using System.IO;
using Microsoft.Win32.SafeHandles;

namespace FOG
{
    /// <summary>
    ///     A generic pipe server client
    /// </summary>
    public class Client
    {
        private FileStream fileStream;
        private SafeFileHandle safeFileHandle;

        public Client(SafeFileHandle safeFileHandle, FileStream fileStream)
        {
            this.safeFileHandle = safeFileHandle;
            this.fileStream = fileStream;
        }

        public Client()
        {
        }

        public SafeFileHandle getSafeFileHandle()
        {
            return safeFileHandle;
        }

        public void setFileHandle(SafeFileHandle safeFilHandle)
        {
            safeFileHandle = safeFilHandle;
        }

        public FileStream getFileStream()
        {
            return fileStream;
        }

        public void setFileStream(FileStream fileStream)
        {
            this.fileStream = fileStream;
        }
    }
}