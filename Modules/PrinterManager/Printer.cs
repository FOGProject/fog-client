using System.Diagnostics;
using System.Runtime.InteropServices;
using FOG.Handlers;

namespace FOG.Modules
{
    public abstract class Printer
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDefaultPrinter(string name);

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
        public string LogName { get; protected set; }

        public abstract void Add();

        public static void Remove(string name)
        {
            LogHandler.Log("Printer", "Removing printer: " + name);
            var proc = name.StartsWith("\\\\")
                ? Process.Start("rundll32.exe", string.Format(" printui.dll,PrintUIEntry /gd /q /n \"{0}\"", name))
                : Process.Start("rundll32.exe", string.Format(" printui.dll,PrintUIEntry /dl /q /n \"{0}\"", name));

            if (proc == null) return;
            proc.Start();
            proc.WaitForExit(120000);
        }

        public void Remove()
        {
            Remove(Name);
        }

        public void SetDefault()
        {
            LogHandler.Log("Printer", "Setting default: " + Name);

            SetDefaultPrinter(Name);
        }
    }


    

}
