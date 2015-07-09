using System;
using FOG.Handlers;
using FOG.Handlers.Middleware;

namespace FOG.Commands.Core.Middleware
{
    class ConfigurationCommand : ICommand
    {
        private const string LogName = "Console::Configuration";
        private const string Server = "http://fog.jbob.io/fog";
        private const string MAC = "1a:2b:3c:4d:5e:6f";

        public bool Process(string[] args)
        {
            if (args[0].Equals("info"))
            {
                Log.Entry(LogName, "Server: " + Configuration.ServerAddress);
                Log.Entry(LogName, "MAC: " + Configuration.MACAddresses());
                return true;
            }

            if (args[0].Equals("configure"))
            {
                if (args[1].Equals("default"))
                {
                    Configuration.ServerAddress = Server;
                    Configuration.TestMAC = MAC;
                    return true;
                }
                if (args.Length < 3) return false;
                if (args[1].Equals("server"))
                {
                    Configuration.ServerAddress = args[2];
                    return true;
                }
                if (args[1].Equals("mac"))
                {
                    Configuration.TestMAC = args[2];
                    return true;
                }
            }

            return false;
        }
    }
}
