using System;
using FOG.Handlers;

namespace FOG.Modules.PrinterManager
{
    public abstract class Printer
    {

        public enum PrinterType
        {
            // ReSharper disable once InconsistentNaming
            iPrint,
            Network,
            Local
        }

        //Basic variables for printers
        public string Port { get; protected set; }
        public string File { get; protected set; }
        public string Model { get; protected set; }
        public string Name { get; protected set; }
        public string IP { get; protected set; }
        public bool Default { get; protected set; }
        public PrinterType Type { get; protected set; }
        public static string LogName { get; protected set; }

        public abstract void Add(PrintManagerBridge instance);

        public void Remove(PrintManagerBridge instance)
        {
            try
            {
                instance.Remove(Name);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not remove");
                Log.Error(LogName, ex);
            }
        }

        public void SetDefault(PrintManagerBridge instance)
        {
            Log.Entry("Printer", "Setting default: " + Name);
            try
            {
                instance.Default(Name);
            }
            catch (Exception ex)
            {
                Log.Error("Printer", ex);
            }
        }
    }
}
