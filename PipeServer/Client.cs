using System.IO;
using Microsoft.Win32.SafeHandles;

namespace FOG
{
    /// <summary>
    ///     A generic pipe server client
    /// </summary>
    public class Client
    {
        private FileStream _fileStream;
        private SafeFileHandle _safeFileHandle;

        public Client(SafeFileHandle safeFileHandle, FileStream fileStream)
        {
            _safeFileHandle = safeFileHandle;
            _fileStream = fileStream;
        }

        public Client()
        {
        }

        public SafeFileHandle GetSafeFileHandle()
        {
            return _safeFileHandle;
        }

        public void SetFileHandle(SafeFileHandle safeFilHandle)
        {
            _safeFileHandle = safeFilHandle;
        }

        public FileStream GetFileStream()
        {
            return _fileStream;
        }

        public void SetFileStream(FileStream fileStream)
        {
            _fileStream = fileStream;
        }
    }
}