
using System;
using System.Collections.Generic;

namespace FOG {
	/// <summary>
	/// Handle all notifications
	/// </summary>
	public static class NotificationHandler
	{
		//Define variable
		private static List<Notification> notifications = new List<Notification>();
		private static String companyName = "FOG";
				
		
		public static void SetCompanyName(String name) {companyName = name; }
		public static String GetCompanyName() { return companyName; }		
				
		public static void CreateNotification(Notification notification) { GetNotifications().Add(notification); }
		public static List<Notification> GetNotifications() { return notifications; }
		public static void ClearNotifications() { GetNotifications().Clear(); }
		public static void RemoveNotification(Notification notification) { GetNotifications().Remove(notification); }
		public static void RemoveNotification(int index) { GetNotifications().RemoveAt(index); }
		
	}
}