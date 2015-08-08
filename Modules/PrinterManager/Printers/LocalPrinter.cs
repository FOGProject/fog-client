using FOG.Handlers;

namespace FOG.Modules.PrinterManager
{
    public class LocalPrinter : Printer
    {
        public LocalPrinter(string name, string file, string port, string ip, string model, bool defaulted)
        {
            Name = name;
            Port = port;
            File = file;
            Model = model;
            Default = defaulted;
            IP = ip;
            LogName = "LocalPrinter";
        }

        public override void Add(PrintManagerBridge instance)
        {
            Log.Entry(LogName, "Attempting to add printer:");
            Log.Entry(LogName, string.Format("--> Name = {0}", Name));
            if(IP != null)
                Log.Entry(LogName, string.Format("--> IP = {0}", IP));
            Log.Entry(LogName, string.Format("--> Port = {0}", Port));
            Log.Entry(LogName, string.Format("--> File = {0}", File));
            Log.Entry(LogName, string.Format("--> Model = {0}", Model));

            instance.Add(this);
        }
    }
}
