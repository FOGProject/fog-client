using System;
using FOG.Handlers;
using Newtonsoft.Json.Linq;

namespace FOG.Commands.Core.CBus
{
    class BusCommand : ICommand
    {
        private const string LogName = "Console::Bus";

        public bool Process(string[] args)
        {
            if (args.Length == 2 && args[1].ToLower().Equals("mode"))
            {
                if(args[1].Equals("server"))
                    Bus.SetMode(Bus.Mode.Server);
                else if (args[1].Equals("client"))
                    Bus.SetMode(Bus.Mode.Client);
            }
            else if (args.Length == 2 && args[1].ToLower().Equals("public"))
            {
                dynamic json = new JObject();
                json.content = args[2];
                Bus.Emit(Bus.Channel.Debug, json, true);
            }
            else if (args.Length == 2 && args[1].ToLower().Equals("private"))
            {
                dynamic json = new JObject();
                json.content = args[2];
                Bus.Emit(Bus.Channel.Debug, json, false);
            }

            return false;
        }

    }
}
