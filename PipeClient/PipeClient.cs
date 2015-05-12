using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace FOG
{
    /// <summary>
    ///     Inter-proccess communication client
    /// </summary>
    public class PipeClient
    {
        public delegate void MessageReceivedHandler(string message);

        //Define variables
        private const uint GENERIC_READ = (0x80000000);
        private const uint GENERIC_WRITE = (0x40000000);
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_FLAG_OVERLAPPED = (0x40000000);
        private const int BUFFER_SIZE = 4096;
        private readonly string pipeName;
        private bool connected;
        private SafeFileHandle handle;
        private Thread readThread;
        private FileStream stream;

        public PipeClient(string pipeName)
        {
            connected = false;
            this.pipeName = pipeName;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string pipeName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplate);

        public event MessageReceivedHandler MessageReceived;

        public bool isConnected()
        {
            return connected;
        }

        public string getPipeName()
        {
            return pipeName;
        }

        //Connect to a server using the same pipe
        public bool connect()
        {
            try
            {
                handle = CreateFile(@"\\.\pipe\" + pipeName, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero,
                    OPEN_EXISTING, FILE_FLAG_OVERLAPPED, IntPtr.Zero);

                if (handle == null)
                    return false;

                if (handle.IsInvalid)
                {
                    connected = false;
                    return false;
                }

                connected = true;

                readThread = new Thread(readFromPipe);
                readThread.Start();

                return true;
            }
            catch
            {
                return false;
            }
        }

        //Stop the pipe client
        public void kill()
        {
            try
            {
                if (stream != null)
                    stream.Close();

                if (handle != null)
                    handle.Close();

                readThread.Abort();
            }
            catch
            {
                // ignored
            }
        }

        //Read a message sent over from the pipe server
        public void readFromPipe()
        {
            stream = new FileStream(handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            var readBuffer = new byte[BUFFER_SIZE];

            var encoder = new ASCIIEncoding();
            while (true)
            {
                var bytesRead = 0;

                try
                {
                    bytesRead = stream.Read(readBuffer, 0, BUFFER_SIZE);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                    break;

                if (MessageReceived != null)
                    MessageReceived(encoder.GetString(readBuffer, 0, bytesRead));
            }
            stream.Close();
            handle.Close();
        }

        //Send a message across the pipe
        public void sendMessage(string message)
        {
            try
            {
                var encoder = new ASCIIEncoding();
                var messageBuffer = encoder.GetBytes(message);

                stream.Write(messageBuffer, 0, messageBuffer.Length);
                stream.Flush();
            }
            catch
            {
            }
        }
    }
}