using System;
using System.Collections.Generic;

namespace FOG
{
    /// <summary>
    /// Handle all notifications
    /// </summary>
    public static class NotificationHandler
    {
        //Define variable
        public static List<Notification> Notifications { get; set; }
        public static String Company { get; private set; }
        private static Boolean initialized = initialize();
	

        private static Boolean initialize() {
            Notifications = new List<Notification>();
            Company = "FOG";
            
            return true;
        }
    }
}