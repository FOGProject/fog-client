
using System;

namespace FOG
{
    /// <summary>
    /// Reboot the computer if a task needs to
    /// </summary>
    public class TaskReboot : AbstractModule
    {
		
        private Boolean notifiedUser;
        //This variable is used to detect if the user has been told their is a pending shutdown
		
        public TaskReboot()
        {
            Name = "TaskReboot";
            Description = "Reboot if a task is scheduled";
            this.notifiedUser = false;
        }
		
        protected override void doWork()
        {
            //Get task info
            var taskResponse = CommunicationHandler.GetResponse("/service/jobs.php", true);

            //Shutdown if a task is avaible and the user is logged out or it is forced
            if (!taskResponse.Error)
            {
                LogHandler.Log(Name, "Restarting computer for task");
                if (!UserHandler.IsUserLoggedIn() || taskResponse.getField("#force").Equals("1"))
                {
                    ShutdownHandler.Restart(Name, 30);
                }
                else if (!taskResponse.Error && !this.notifiedUser)
                {
                    LogHandler.Log(Name, "User is currently logged in, will try again later");
                    NotificationHandler.Notifications.Add(new Notification("Please log off", NotificationHandler.Company +
                            " is attemping to service your computer, please log off at the soonest available time",
                            60));
                    this.notifiedUser = true;
                }
            }
			
        }
		
    }
}