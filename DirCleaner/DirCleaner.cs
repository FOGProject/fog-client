
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
			Response dirResponse = CommunicationHandler.getResponse("/service/dircleanup-dirs.php?mac=" + CommunicationHandler.getMacAddresses());
			
			//Shutdown if a task is avaible and the user is logged out or it is forced
			if(!dirResponse.wasError()) {
				foreach(String dir in CommunicationHandler.parseDataArray(dirResponse, "#dir", true)) {
					
					try {
						LogHandler.log(getName(), "Deleting " + Environment.ExpandEnvironmentVariables(dir));
						Directory.Delete(Environment.ExpandEnvironmentVariables(dir),true);
						
					} catch (Exception ex) {
						LogHandler.log(getName(), "ERROR: " + ex.Message);
					}
				}
			}
			
		}		
			
	}
}