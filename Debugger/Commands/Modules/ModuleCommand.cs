using System;
using System.Collections.Generic;
using FOG.Handlers;
using FOG.Modules;
using FOG.Modules.AutoLogOut;

namespace FOG.Commands.Modules
{
    class ModuleCommand : ICommand
    {
        private readonly Dictionary<string, AbstractModule> _modules = new Dictionary<string, AbstractModule>();
        private const string LogName = "Console::Modules";

        private void AddModule(string module)
        {
            try
            {
                switch (module.ToLower())
                {
                    case "autologout":
                        _modules.Add(module, new AutoLogOut());
                        break;
                    default:
                        Log.Error(LogName, "Unknown module name");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to add " + module);
                Log.Error(LogName, ex);
            }
        }


        public bool Process(string[] args)
        {
            if (_modules.ContainsKey(args[0].ToLower()))
            {
                _modules[args[0]].Start();
                return true;
            }

            AddModule(args[0]);
            if (!_modules.ContainsKey(args[0].ToLower())) return false;
               
            _modules[args[0]].Start();
            return true;
        }
    }
}
