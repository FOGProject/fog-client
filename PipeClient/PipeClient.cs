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
        private const uint GenericRead = (0x80000000);
        private const uint GenericWrite = (0x40000000);
        private const uint OpenExisting = 3;
        private const uint FileFlagOverlapped = (0x40000000);
        private const int BufferSize = 4096;
        private readonly string _pipeName;
        private bool _connected;
        private SafeFileHandle _handle;
        private Thread _readThread;
        private FileStream _stream;

        public PipeClient(string pipeName)
        {
            _connected = false;
            _pipeName = pipeName;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string pipeName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplate);

        public event MessageReceivedHandler MessageReceived;

        public bool IsConnected()
        {
            return _connected;
        }

        public string GetPipeName()
        {
            return _pipeName;
        }

        //Connect to a server using the same pipe
        public bool Connect()
        {
            try
            {
                _handle = CreateFile(@"\\.\pipe\" + _pipeName, GenericRead | GenericWrite, 0, IntPtr.Zero,
                    OpenExisting, FileFlagOverlapped, IntPtr.Zero);

                if (_handle == null)
                    return false;

                if (_handle.IsInvalid)
                {
                    _connected = false;
                    return false;
                }

                _connected = true;

                _readThread = new Thread(ReadFromPipe);
                _readThread.Start();

                return true;
            }
            catch
            {
                return false;
            }
        }

        //Stop the pipe client
        public void Kill()
        {
            try
            {
                if (_stream != null)
                    _stream.Close();

                if (_handle != null)
                    _handle.Close();

                _readThread.Abort();
            }
            catch
            {
                // ignored
            }
        }

        //Read a message sent over from the pipe server
        public void ReadFromPipe()
        {
            _stream = new FileStream(_handle, FileAccess.ReadWrite, BufferSize, true);
            var readBuffer = new byte[BufferSize];

            var encoder = new ASCIIEncoding();
            while (true)
            {
                var bytesRead = 0;

                try
                {
                    bytesRead = _stream.Read(readBuffer, 0, BufferSize);
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
            _stream.Close();
            _handle.Close();
        }

        //Send a message across the pipe
        public void SendMessage(string message)
        {
            try
            {
                var encoder = new ASCIIEncoding();
                var messageBuffer = encoder.GetBytes(message);

                _stream.Write(messageBuffer, 0, messageBuffer.Length);
                _stream.Flush();
            }
            catch
            {
                // ignored
            }
        }
    }
}