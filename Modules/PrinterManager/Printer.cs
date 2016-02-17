using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
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
        public string ConfigFile { get; protected set; }
        public string Model { get; protected set; }
        public string Name { get; protected set; }
        public string IP { get; protected set; }
        public bool Default { get; protected set; }
        public PrinterType Type { get; protected set; }
        public static string LogName { get; protected set; }

        public abstract void Add();

        public static void Remove(string name)
        {
            try
            {
                Log.Entry("Printer", "Removing printer: " + name);
                var proc = name.StartsWith("\\\\")
                    ? Process.Start("rundll32.exe", string.Format(" printui.dll,PrintUIEntry /gd /q /n \"{0}\"", name))
                    : Process.Start("rundll32.exe", string.Format(" printui.dll,PrintUIEntry /dl /q /n \"{0}\"", name));

                if (proc == null) return;
                proc.Start();
                proc.WaitForExit(120000);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not remove");
                Log.Error(LogName, ex);
            }

        }

        public void Remove()
        {
            Remove(Name);
        }

        public void SetDefault()
        {
            Log.Entry("Printer", "Setting default: " + Name);

            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

            var collection = searcher.Get();


            foreach (var currentObject in collection.Cast<ManagementObject>().Where(currentObject => currentObject["name"].ToString().Equals(Name)))
                currentObject.InvokeMethod("SetDefaultPrinter", new object[] {Name});
        }
    }
}
