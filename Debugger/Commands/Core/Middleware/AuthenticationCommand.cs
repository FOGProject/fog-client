using FOG.Handlers;
using FOG.Handlers.Middleware;

namespace FOG.Commands.Core.Middleware
{
    class AuthenticationCommand : ICommand
    {
        private const string LogName = "Console::Authentication";

        public bool Process(string[] args)
        {
            if (args[0].Equals("handshake"))
            {
                Authentication.HandShake();
                return true;
            }

            return false;
        }
    }
}
