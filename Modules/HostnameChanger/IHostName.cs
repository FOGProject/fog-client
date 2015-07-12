using FOG.Handlers.Middleware;

namespace FOG.Modules.HostnameChanger
{
    interface IHostName
    {
        void RenameComputer(string hostname);
        bool RegisterComputer(Response response);
        void UnRegisterComputer(Response response);
        void ActivateComputer(string key);
    }
}
