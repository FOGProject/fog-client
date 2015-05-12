using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace FOG
{
    /// <summary>
    ///     Inter-proccess communication server
    /// </summary>
    public class PipeServer
    {
        public delegate void MessageReceivedHandler(Client client, string message);

        private const uint Duplex = (0x00000003);
        private const uint FileFlagOverlapped = (0x40000000);
        public const int BufferSize = 4096;
        private readonly List<Client> _clients;
        private readonly string _pipeName;
        private Thread _listenThread;
        private bool _running;

        public PipeServer(string pipeName)
        {
            _running = false;
            _pipeName = pipeName;
            _clients = new List<Client>();
        }

        //Import DLL functions
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(string pipeName, uint dwOpenMode, uint dwPipeMode,
            uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut,
            IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(SafeFileHandle hNamedPipe, IntPtr lpOverlapped);

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern bool InitializeSecurityDescriptor(out SecurityDescriptor sd, int dwRevision);

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern bool SetSecurityDescriptorDacl(ref SecurityDescriptor sd, bool bDaclPresent, IntPtr dacl,
            bool bDaclDefaulted);

        public event MessageReceivedHandler MessageReceived;

        public bool IsRunning()
        {
            return _running;
        }

        public string GetPipeName()
        {
            return _pipeName;
        }

        //start the pipe server
        public void Start()
        {
            _listenThread = new Thread(ListenForClients) {IsBackground = true};
            _listenThread.Start();
            _running = true;
        }

        //Wait for a client to try and connect
        private void ListenForClients()
        {
            var ptrSec = CreateSecurity();

            while (true)
            {
                var clientHandle = CreateNamedPipe(@"\\.\pipe\" + _pipeName, Duplex | FileFlagOverlapped,
                    0, 255, BufferSize, BufferSize, 0, ptrSec);

                if (clientHandle.IsInvalid)
                    return;

                var success = ConnectNamedPipe(clientHandle, IntPtr.Zero);

                if (success == 0)
                    return;

                var client = new Client();
                client.SetFileHandle(clientHandle);

                lock (_clients)
                    _clients.Add(client);

                var readThread = new Thread(Read) {IsBackground = true};
                readThread.Start(client);
            }
        }

        //Change the security settings of the pipe so the SYSTEM ACCOUNT can interact with user accounts
        private static IntPtr CreateSecurity()
        {
            var ptrSec = IntPtr.Zero;
            var securityAttribute = new SecurityAttributes();
            SecurityDescriptor securityDescription;

            if (!InitializeSecurityDescriptor(out securityDescription, 1)) return ptrSec;
            if (!SetSecurityDescriptorDacl(ref securityDescription, true, IntPtr.Zero, false)) return ptrSec;
            
            securityAttribute.lpSecurityDescriptor =
                Marshal.AllocHGlobal(Marshal.SizeOf(typeof (SecurityDescriptor)));
            Marshal.StructureToPtr(securityDescription, securityAttribute.lpSecurityDescriptor, false);
            securityAttribute.bInheritHandle = false;
            securityAttribute.nLength = Marshal.SizeOf(typeof (SecurityAttributes));
            ptrSec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (SecurityAttributes)));
            Marshal.StructureToPtr(securityAttribute, ptrSec, false);
            return ptrSec;
        }

        //Read a message sent over the pipe
        private void Read(object objClient)
        {
            var client = (Client) objClient;
            client.SetFileStream(new FileStream(client.GetSafeFileHandle(), FileAccess.ReadWrite, BufferSize, true));

            var buffer = new byte[BufferSize];
            var encoder = new ASCIIEncoding();

            while (true)
            {
                var bRead = 0;

                try
                {
                    bRead = client.GetFileStream().Read(buffer, 0, BufferSize);
                }
                catch
                {
                    // ignored
                }

                if (bRead == 0)
                    break;

                if (MessageReceived != null)
                    MessageReceived(client, encoder.GetString(buffer, 0, bRead));
            }

            client.GetFileStream().Close();
            client.GetFileStream().Close();
            lock (_clients)
                _clients.Remove(client);
        }

        //Send a message across the pipe
        public void SendMessage(string msg)
        {
            lock (_clients)
            {
                var encoder = new ASCIIEncoding();
                var mBuf = encoder.GetBytes(msg);

                foreach (var client in _clients)
                {
                    client.GetFileStream().Write(mBuf, 0, mBuf.Length);
                    client.GetFileStream().Flush();
                }
            }
        }

        //Define variables
        [StructLayout(LayoutKind.Sequential)]
        public struct SecurityAttributes
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SecurityDescriptor
        {
            private readonly byte Revision;
            private readonly byte Sbz1;
            private readonly ushort Control;
            private readonly IntPtr Owner;
            private readonly IntPtr Group;
            private readonly IntPtr Sacl;
            private readonly IntPtr Dacl;
        }
    }
}