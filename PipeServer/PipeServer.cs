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

        private const uint DUPLEX = (0x00000003);
        private const uint FILE_FLAG_OVERLAPPED = (0x40000000);
        public const int BUFFER_SIZE = 4096;
        private readonly List<Client> clients;
        private readonly string pipeName;
        private Thread listenThread;
        private bool running;

        public PipeServer(string pipeName)
        {
            running = false;
            this.pipeName = pipeName;
            clients = new List<Client>();
        }

        //Import DLL functions
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(string pipeName, uint dwOpenMode, uint dwPipeMode,
            uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut,
            IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(SafeFileHandle hNamedPipe, IntPtr lpOverlapped);

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern bool InitializeSecurityDescriptor(out SECURITY_DESCRIPTOR sd, int dwRevision);

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern bool SetSecurityDescriptorDacl(ref SECURITY_DESCRIPTOR sd, bool bDaclPresent, IntPtr Dacl,
            bool bDaclDefaulted);

        public event MessageReceivedHandler MessageReceived;

        public bool isRunning()
        {
            return running;
        }

        public string getPipeName()
        {
            return pipeName;
        }

        //start the pipe server
        public void start()
        {
            listenThread = new Thread(listenForClients) {IsBackground = true};
            listenThread.Start();
            running = true;
        }

        //Wait for a client to try and connect
        private void listenForClients()
        {
            var ptrSec = createSecurity();

            while (true)
            {
                var clientHandle = CreateNamedPipe(@"\\.\pipe\" + pipeName, DUPLEX | FILE_FLAG_OVERLAPPED,
                    0, 255, BUFFER_SIZE, BUFFER_SIZE, 0, ptrSec);

                if (clientHandle.IsInvalid)
                    return;

                var success = ConnectNamedPipe(clientHandle, IntPtr.Zero);

                if (success == 0)
                    return;

                var client = new Client();
                client.setFileHandle(clientHandle);

                lock (clients)
                    clients.Add(client);

                var readThread = new Thread(read);
                readThread.IsBackground = true;
                readThread.Start(client);
            }
        }

        //Change the security settings of the pipe so the SYSTEM ACCOUNT can interact with user accounts
        private static IntPtr createSecurity()
        {
            var ptrSec = IntPtr.Zero;
            var securityAttribute = new SECURITY_ATTRIBUTES();
            SECURITY_DESCRIPTOR securityDescription;

            if (!InitializeSecurityDescriptor(out securityDescription, 1)) return ptrSec;
            if (!SetSecurityDescriptorDacl(ref securityDescription, true, IntPtr.Zero, false)) return ptrSec;
            
            securityAttribute.lpSecurityDescriptor =
                Marshal.AllocHGlobal(Marshal.SizeOf(typeof (SECURITY_DESCRIPTOR)));
            Marshal.StructureToPtr(securityDescription, securityAttribute.lpSecurityDescriptor, false);
            securityAttribute.bInheritHandle = false;
            securityAttribute.nLength = Marshal.SizeOf(typeof (SECURITY_ATTRIBUTES));
            ptrSec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (SECURITY_ATTRIBUTES)));
            Marshal.StructureToPtr(securityAttribute, ptrSec, false);
            return ptrSec;
        }

        //Read a message sent over the pipe
        private void read(object objClient)
        {
            var client = (Client) objClient;
            client.setFileStream(new FileStream(client.getSafeFileHandle(), FileAccess.ReadWrite, BUFFER_SIZE, true));

            var buffer = new byte[BUFFER_SIZE];
            var encoder = new ASCIIEncoding();

            while (true)
            {
                var bRead = 0;

                try
                {
                    bRead = client.getFileStream().Read(buffer, 0, BUFFER_SIZE);
                }
                catch
                {
                }

                if (bRead == 0)
                    break;

                if (MessageReceived != null)
                    MessageReceived(client, encoder.GetString(buffer, 0, bRead));
            }

            client.getFileStream().Close();
            client.getFileStream().Close();
            lock (clients)
                clients.Remove(client);
        }

        //Send a message across the pipe
        public void sendMessage(string msg)
        {
            lock (clients)
            {
                var encoder = new ASCIIEncoding();
                var mBuf = encoder.GetBytes(msg);

                foreach (var client in clients)
                {
                    client.getFileStream().Write(mBuf, 0, mBuf.Length);
                    client.getFileStream().Flush();
                }
            }
        }

        //Define variables
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_DESCRIPTOR
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