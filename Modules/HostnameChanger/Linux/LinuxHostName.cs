using System;
using FOG.Handlers.Middleware;

namespace FOG.Modules.HostnameChanger.Linux
{
    class LinuxHostName : IHostName
    {
        public void RenameComputer(string hostname)
        {
            throw new NotImplementedException();
        }

        public bool RegisterComputer(Response response)
        {
            throw new NotImplementedException();
        }

        public void UnRegisterComputer(Response response)
        {
            throw new NotImplementedException();
        }

        public void ActivateComputer(string key)
        {
            throw new NotImplementedException();
        }
    }
}
