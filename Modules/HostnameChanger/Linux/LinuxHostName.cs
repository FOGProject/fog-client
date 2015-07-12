using System;
using System.Diagnostics;
using FOG.Handlers;
using FOG.Handlers.Middleware;

namespace FOG.Modules.HostnameChanger.Linux
{
    class LinuxHostName : IHostName
    {
        private string Name = "HostnameChanger";

        public void RenameComputer(string hostname)
        {
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
