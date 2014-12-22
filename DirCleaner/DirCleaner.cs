
using System;
using System.IO;

using System.Collections.Generic;

namespace FOG {
	/// <summary>
	/// Delete specified directories
	/// </summary>
	public class DirCleaner : AbstractModule {
		public DirCleaner():base(){
			setName("DirCleaner");
			setDescription("Delete specified directories");

		}
			
		protected override void doWork() {
			//Get directories to delete
			Response dirResponse = CommunicationHandler.GetResponse("/service/dircleanup-dirs.php?mac=" + CommunicationHandler.GetMacAddresses());
			
			//Shutdown if a task is avaible and the user is logged out or it is forced
			if(!dirResponse.wasError()) {
				foreach(String dir in CommunicationHandler.ParseDataArray(dirResponse, "#dir", true)) {
					
					try {
						LogHandler.Log(getName(), "Deleting " + Environment.ExpandEnvironmentVariables(dir));
						Directory.Delete(Environment.ExpandEnvironmentVariables(dir),true);
						
					} catch (Exception ex) {
						LogHandler.Log(getName(), "ERROR: " + ex.Message);
					}
				}
			}
			
		}		
			
	}
}