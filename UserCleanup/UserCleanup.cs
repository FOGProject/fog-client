
using System;
using System.Linq;
using System.Collections.Generic;

namespace FOG {
	/// <summary>
	/// Remove specified users
	/// </summary>

	public class UserCleanup : AbstractModule {
		
		public UserCleanup():base(){
			setName("UserCleanup");
			setDescription("Remove specified users");
		}
		
		protected override void doWork() {
			//Get task info
			Response usersResponse = CommunicationHandler.GetResponse("/service/usercleanup-users.php?mac=" + CommunicationHandler.GetMacAddresses());

			if(!usersResponse.wasError()) {
				List<String> protectedUsers = CommunicationHandler.ParseDataArray(usersResponse, "#user", false);
				
				if(protectedUsers.Count > 0) {
					foreach(UserData user in UserHandler.GetAllUserData()) {
						if(!protectedUsers.Contains(user.GetName(), StringComparer.OrdinalIgnoreCase) && !UserHandler.GetUsersLoggedIn().Contains(user.GetName(), StringComparer.OrdinalIgnoreCase)) {
							UserHandler.PurgeUser(user, true);
						} else {
							LogHandler.Log(getName(), user.GetName() + " is either logged in or protected, skipping");
						}
					}
				}
			}
			
		}
		
	}
}