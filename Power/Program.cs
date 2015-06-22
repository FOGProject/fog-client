using FOG.Handlers;
using Newtonsoft.Json.Linq;

namespace FOG
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Output = Log.Mode.Console;

            if (args.Length <= 0) return;

            dynamic json = new JObject();
            json.action = "help";

            if (args[0].Equals("shutdown"))
                json.type = "shutdown";
            else if(args[0].Equals("reboot"))
                json.type = "reboot";

            if (args.Length > 1)
                json.reason = args[1];

            Bus.Emit(Bus.Channel.Power, json, true);
        }
    }
}
