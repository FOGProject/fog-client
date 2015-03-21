using System;
using System.Threading;

namespace FOG
{
    /// <summary>
    /// Automatically log out the user after X seconds of inactivity
    /// </summary>
    public class AutoLogOut: AbstractModule
    {

        private int minimumTime;
        public AutoLogOut() : base()
        {
            setName("AutoLogOut");
            setDescription("Automatically log out the user if they are inactive");
            this.minimumTime = 300;
        }

        protected override void doWork()
        {
            if (UserHandler.IsUserLoggedIn()) {
                //Get task info
                var taskResponse = CommunicationHandler.GetResponse("/service/autologout.php?mac=" + CommunicationHandler.GetMacAddresses());

                if (!taskResponse.wasError()) {
                    int timeOut = getTimeOut(taskResponse);
                    if (timeOut > 0) {
                        LogHandler.Log(getName(), "Time set to " + timeOut.ToString() + " seconds");
                        LogHandler.Log(getName(), "Inactive for " + UserHandler.GetUserInactivityTime().ToString() + " seconds");
                        if (UserHandler.GetUserInactivityTime() >= timeOut) {
                            NotificationHandler.CreateNotification(new Notification("You are about to be logged off",
                                "Due to inactivity you will be logged off if you remain inactive", 20));
                            //Wait 20 seconds and check if the user is no longer inactive
                            Thread.Sleep(20000);
                            if (UserHandler.GetUserInactivityTime() >= timeOut)
                                ShutdownHandler.LogOffUser();
                        }
                    }

                }
            } else {
                LogHandler.Log(getName(), "No user logged in");
            }

        }

        //Get how long a user must be inactive before logging them out
        private int getTimeOut(Response taskResponse)
        {
            try {
                int timeOut = int.Parse(taskResponse.getField("#time"));
                if (timeOut >= this.minimumTime) {
                    return timeOut;
                }
              
                LogHandler.Log(getName(), "Time set is less than 1 minute");

            } catch (Exception ex) {
                LogHandler.Log(getName(), "Unable to parsing time set");
                LogHandler.Log(getName(), "ERROR: " + ex.Message);
            }

            return 0;

        }

    }
}