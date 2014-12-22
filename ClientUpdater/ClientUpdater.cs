
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;


namespace FOG
{
	/// <summary>
	/// Update the FOG Service
	/// </summary>
	public class ClientUpdater : AbstractModule {
		
		private Boolean updatePending;
		
		public ClientUpdater() : base() {
			setName("ClientUpdater");
			setDescription("Update the FOG Service");
			this.updatePending = false;
			
		}
		
		protected override void doWork() {
			this.updatePending = false;
			//Get task info
			Response updateResponse = CommunicationHandler.GetResponse("/service/updates.php?action=list");	
			if(!updateResponse.wasError()) {
				List<String> updates = CommunicationHandler.ParseDataArray(updateResponse, "#update", true);
				
				//Loop through each update file and compare its hash to the local copy
				foreach(String updateFile in updates) {
					LogHandler.Log(getName(), "Possible update for " + updateFile + " found");
					Response askResponse = CommunicationHandler.GetResponse("/service/updates.php?action=ask&file=" + 
					                                                         EncryptionHandler.EncodeBase64(updateFile));
					
					//Check if the response is correct
					if(!askResponse.wasError() && !askResponse.getField("#md5").Equals("")) {
						String updateFileHash = askResponse.getField("#md5");;
						
						//Check if the MD5 hashes are note equal
						if(!EncryptionHandler.GetMD5Hash(AppDomain.CurrentDomain.BaseDirectory 
						                                      + @"\" + updateFile).Equals(updateFileHash)) {
							
							LogHandler.Log(getName(), "Remote file is newer, attempting to update");
							
							if(generateUpdateFile(askResponse.getField("#md5"), updateFile))
								prepareUpdaste(updateFile);
						} else {
							LogHandler.Log(getName(), "Remote file is the same as this local copy");
						}
					}
				}
				if(updatePending) {
					ShutdownHandler.ScheduleUpdate();
				}
			}
		}
		
		//Generate the update file from the parsed response
		private Boolean generateUpdateFile(String md5, String updateFile) {
			LogHandler.Log(getName(), "Downloading update file");
			//Download the new file
			Response updateFileResponse = CommunicationHandler.GetResponse("/service/updates.php?action=get&file=" + 
			                                                               EncryptionHandler.EncodeBase64(updateFile));
					                                                         
			if(!updateFileResponse.getField("#updatefile").Equals("")) {
				
				try {
					
					//Create the directory that the file will go in if it doesn't already exist
					if(!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"tmp\")) {
						Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"tmp\");
					}
					
					if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + updateFile))
						File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + updateFile);
					
					File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + updateFile, EncryptionHandler.StringToByteArray(updateFileResponse.getField("#updatefile")));
					LogHandler.Log(getName(), "Verifying MD5 hash");
					
					if(EncryptionHandler.GetMD5Hash(AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + updateFile).Equals(md5)) {
						return true;
					} else {
						LogHandler.Log(getName(), "Failure");
						LogHandler.Log(getName(), "SVR: " + md5);
						LogHandler.Log(getName(), "DWD: " + EncryptionHandler.GetMD5Hash(AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + updateFile));
						File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + updateFile);
					}
				} catch (Exception ex) {
					LogHandler.Log(getName(), "Unable to generate update file");
					LogHandler.Log(getName(), "ERROR: " + ex.Message);
				}

			}
			return false;
		}
		
		//Prepare the downloaded update
		private void prepareUpdaste(String updateFile) {
			if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + updateFile)) {
							
				try {
					try {
						if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\" + updateFile))
							File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\" + updateFile);
						
						File.Move(AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + updateFile, AppDomain.CurrentDomain.BaseDirectory + @"\" + updateFile);
						LogHandler.Log(getName(), "Successfully applied " + updateFile + ", no service restart needed for this update");   
					} catch (Exception) {
						if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\" + updateFile + ".update"))
							File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\" + updateFile + ".update");
							
						File.Move(AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + updateFile, AppDomain.CurrentDomain.BaseDirectory + @"\" + updateFile + ".update");
						this.updatePending = true;
						LogHandler.Log(getName(), "Successfully prepared " + updateFile + " for updating, a service restart is required for this update");		
					}
						
				} catch (Exception ex) {
					LogHandler.Log(getName(), "Unable to prepare " + updateFile);
					LogHandler.Log(getName(), "ERROR: " + ex.Message);
				}
			} else {
				LogHandler.Log(getName(), "Unable to locate downloaded update file");
			}
		}
	}
}