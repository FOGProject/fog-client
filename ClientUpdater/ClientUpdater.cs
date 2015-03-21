using System;
using System.IO;


namespace FOG
{
    /// <summary>
    /// Update the FOG Service
    /// </summary>
    public class ClientUpdater: AbstractModule
    {

        public ClientUpdater() : base()
        {
            setName("ClientUpdater");
            setDescription("Update the FOG Service");

        }

        protected override void doWork()
        {
            String serverVersion = CommunicationHandler.GetRawResponse("/service/getversion.php?clientver");
            String localVersion = RegistryHandler.GetSystemSetting("Version");
            try {
                int server = Int32.Parse(serverVersion.Replace(".", ""));
                int local = Int32.Parse(localVersion.Replace(".", ""));

                if (server > local) {
                    CommunicationHandler.DownloadFile("/client/FOGService.msi", AppDomain.CurrentDomain.BaseDirectory + @"\tmp\FOGService.msi");
                    prepareUpdateHelpers();
                    ShutdownHandler.ScheduleUpdate();
                }
            } catch (Exception ex) {
                LogHandler.Log(getName(), "Unable to parse versions");
                LogHandler.Log(getName(), "ERROR: " + ex.Message);
            }
        }

        //Prepare the downloaded update
        private void prepareUpdateHelpers()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\FOGUpdateHelper.exe") && File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\FOGUpdateWaiter.exe")) {

                try {
                    File.Move(AppDomain.CurrentDomain.BaseDirectory + @"\FOGUpdateHelper.exe", AppDomain.CurrentDomain.BaseDirectory + @"tmp\FOGUpdateHelper.exe");
                    File.Move(AppDomain.CurrentDomain.BaseDirectory + @"\FOGUpdateWaiter.exe", AppDomain.CurrentDomain.BaseDirectory + @"tmp\FOGUpdateWaiter.exe");
                    File.Move(AppDomain.CurrentDomain.BaseDirectory + @"\LogHandler.dll", AppDomain.CurrentDomain.BaseDirectory + @"tmp\LogHandler.dll");
                } catch (Exception ex) {
                    LogHandler.Log(getName(), "Unable to prepare update helpers");
                    LogHandler.Log(getName(), "ERROR: " + ex.Message);
                }
            } else {
                LogHandler.Log(getName(), "Unable to locate helper files");
            }
        }
    }
}