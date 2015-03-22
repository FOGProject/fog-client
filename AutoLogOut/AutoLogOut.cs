using System;
using System.Threading;

namespace FOG
{
    /// <summary>
    /// Automatically log out the user after a given duration of inactivity
    /// </summary>
    public class AutoLogOut: AbstractModule
    {

        private int minimumTime;
        public AutoLogOut()
        {
            Name = "AutoLogOut";
            Description = "Automatically log out the user if they are inactive";
            this.minimumTime = 300;
        }

        protected override void doWork()
        {
            if (UserHandler.IsUserLoggedIn())
            {
                //Get task info
                var taskResponse = CommunicationHandler.GetResponse("/service/autologout.php?mac=" + CommunicationHandler.GetMacAddresses());

                if (!taskResponse.Error)
                {
                    int timeOut = getTimeOut(taskResponse);
                    if (timeOut > 0)
                    {
                        LogHandler.Log(Name, "Time set to " + timeOut.ToString() + " seconds");
                        LogHandler.Log(Name, "Inactive for " + UserHandler.GetUserInactivityTime().ToString() + " seconds");
                        if (UserHandler.GetUserInactivityTime() >= timeOut)
                        {
                            NotificationHandler.Notifications.Add(new Notification("You are about to be logged off",
                                    "Due to inactivity you will be logged off if you remain inactive", 20));
                            //Wait 20 seconds and check if the user is no longer inactive
                            Thread.Sleep(20000);
                            if (UserHandler.GetUserInactivityTime() >= timeOut)
                                ShutdownHandler.LogOffUser();
                        }
                    }

                }
            }
            else
            {
                LogHandler.Log(Name, "No user logged in");
            }

        }

        //Get how long a user must be inactive before logging them out
        private int getTimeOut(Response taskResponse)
        {
            try
            {
                int timeOut = int.Parse(taskResponse.getField("#time"));
                if (timeOut >= this.minimumTime)
                {
                    return timeOut;
                }
              
                LogHandler.Log(Name, "Time set is less than 1 minute");

            }
            catch (Exception ex)
            {
                LogHandler.Log(Name, "Unable to parsing time set");
                LogHandler.Log(Name, "ERROR: " + ex.Message);
            }

            return 0;

        }

    }
}