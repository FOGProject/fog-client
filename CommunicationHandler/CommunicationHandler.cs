using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.IO;
using System.Net.NetworkInformation;


namespace FOG {
	/// <summary>
	/// Handle all communication with the FOG Server
	/// </summary>
	public static class CommunicationHandler {
		//Define variables
		private static String serverAddress = "fog-server";
		private static Dictionary<String, String> returnMessages = loadReturnMessages();

		private const String successCode = "#!ok";
		private const String LOG_NAME = "CommunicationHandler";

		private static string passkey = "";


		//Define all return codes
		private static Dictionary<String, String> loadReturnMessages() {

			Dictionary<String, String> messages = new Dictionary<String, String>();
			messages.Add(successCode, "Success");
			messages.Add("#!db", "Database error");
			messages.Add("#!im", "Invalid MAC address format");
			messages.Add("#!ihc", "Invalid host certificate");			
			messages.Add("#!ih", "Invalid host");
			messages.Add("#!il", "Invalid login");					
			messages.Add("#!it", "Invalid task");				
			messages.Add("#!ng", "Module is disabled globally on the FOG server");
			messages.Add("#!nh", "Module is disabled on the host");
			messages.Add("#!um", "Unknown module ID");
			messages.Add("#!ns", "No snapins");		
			messages.Add("#!nj", "No jobs");	
			messages.Add("#!na", "No actions");				
			messages.Add("#!nf", "No updates");				
			messages.Add("#!time", "Invalid time");	
			messages.Add("#!er", "General error");

			return messages;
		}

		//Getters and setters
		public static void setServerAddress(String address) { serverAddress = address; }
		public static String getPassKey() { return passkey; }
		
		public static void setServerAddress(String HTTPS, String address, String webRoot) { 
			if(HTTPS.Equals("1")) {
				serverAddress = "https://";
			} else {
				serverAddress = "http://";
			}
			serverAddress = serverAddress  + address + webRoot;
		}
		
		public static Boolean getAndSetServerAddress() {
			if(RegistryHandler.getSystemSetting("Server") != null && RegistryHandler.getSystemSetting("WebRoot") != null && 
			   RegistryHandler.getSystemSetting("Tray") != null && RegistryHandler.getSystemSetting("HTTPS") != null) {
				
				CommunicationHandler.setServerAddress(RegistryHandler.getSystemSetting("HTTPS"), 
				                                      RegistryHandler.getSystemSetting("Server"), 
				                                      RegistryHandler.getSystemSetting("WebRoot"));
				return true;
			}
			LogHandler.log(LOG_NAME, "Regisitry keys are not set");
			return false;
		}
		
		
		public static String getServerAddress() { return serverAddress; }		

		
		//Return the response form an address
		public static Response getResponse(String postfix) {

			LogHandler.log(LOG_NAME, "URL: " + getServerAddress() + postfix );

			WebClient webClient = new WebClient();
			try {
				String response = webClient.DownloadString(getServerAddress() + postfix);
				response = decryptAES(response, getPassKey());
				//See if the return code is known
				Boolean messageFound = false;
				foreach(String returnMessage in returnMessages.Keys) {
					if(response.StartsWith(returnMessage)) {
						messageFound=true;
						LogHandler.log(LOG_NAME, "Response: " + returnMessages[returnMessage]);
						break;
					}					
				}
				
				if(!messageFound) {
						LogHandler.log(LOG_NAME, "Unknown Response: " + response.Replace("\n", ""));					
				}
           
				return parseResponse(response);
			} catch (Exception ex) {
				LogHandler.log(LOG_NAME, "Error contacting FOG server");			
				LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return new Response();
		}		
		
		public static String getRawResponse(String postfix) {
			//ID the service as the new one
			if(postfix.Contains(".php?")) {
				postfix = postfix + "&newService=1";
			} else {
				postfix = postfix + "?newService=1";
			}
			LogHandler.log(LOG_NAME, "URL: " + getServerAddress() + postfix );
			
			WebClient webClient = new WebClient();
			try {
				String response = webClient.DownloadString(getServerAddress() + postfix);
				return response;
			} catch (Exception ex) {
				LogHandler.log(LOG_NAME, "Error contacting FOG");			
				LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return "";
			
		}
		
		public static Boolean authenticate() {
			
			try {

				String keyPath = AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + "public.key";
				downloadFile("/management/other/ssl/srvpublic.key", keyPath);
				
				passkey = EncryptionHandler.generatePassword(32).Trim();
				String encryptedKey = EncryptionHandler.encryptRSA(passkey, keyPath);
				
				
				Response authenticationResponse = 
					getResponse("/management/index.php?mac=" +  getMacAddresses() + "&sub=authorize&sym_key=" + encryptedKey);
				
				LogHandler.log(LOG_NAME, authenticationResponse.getReturnCode());

				File.Delete(keyPath);
				if(!authenticationResponse.wasError()) {
					LogHandler.log(LOG_NAME, "Authenticated");			
					return true;
				}
			} catch (Exception ex) {
				LogHandler.log(LOG_NAME, "Error authenticating");			
				LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);					
			}
			
			return false;
		}
		
		private static String decryptAES(String response, String passKey) {
			const String encryptedFlag = "#!en=";
			const String encryptedFlag2 = "#!enkey=";
			
			if(response.StartsWith(encryptedFlag2)) {
				String decryptedResponse = response.Substring(encryptedFlag2.Length);
				response = EncryptionHandler.decodeAES(decryptedResponse, passKey);
			}
			if(response.StartsWith(encryptedFlag)) {
				String decryptedResponse = response.Substring(encryptedFlag.Length);
				response = EncryptionHandler.decodeAES(decryptedResponse, passKey);
			}			
			
			
			return response;
		}		

		//Contact FOG at a url, used for submitting data
		public static Boolean contact(String postfix) {
			//ID the service as the new one
			if(postfix.Contains(".php?")) {
				postfix = postfix + "&newService=1";
			} else {
				postfix = postfix + "?newService=1";
			}			
			LogHandler.log(LOG_NAME,
			               "URL: " + getServerAddress() + postfix);
			WebClient webClient = new WebClient();			

			try {
				webClient.DownloadString(getServerAddress() + postfix);
				return true;
				
			} catch (Exception ex) {
				LogHandler.log(LOG_NAME, "Error contacting FOG");		
				LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);
			}
			return false;
		}

		//Parse the recieved data
		private static Response parseResponse(String rawResponse) {
			String[] data = rawResponse.Split('\n'); //Split the response at every new line
			Dictionary<String, String> parsedData = new Dictionary<String, String>();
			Response response = new Response();

			try {
				//Get and set the error boolean
				String returnCode = data[0];
				response.setReturnCode(returnCode);
				response.setError(!returnCode.ToLower().Trim().StartsWith(successCode));

				//Loop through each line returned and if it contains an '=' add it to the dictionary
				foreach(String element in data) {
					if(element.Contains("=")) {
						parsedData.Add(element.Substring(0, element.IndexOf("=")).Trim(),
						               element.Substring(element.IndexOf("=")+1).Trim());
					}
				}

				response.setData(parsedData);
			} catch (Exception ex) {
				LogHandler.log(LOG_NAME, "Error parsing response");
				LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return response;
		}
		
		//Get an array from a response
		public static List<String> parseDataArray(Response response, String identifier, Boolean base64Decode) {
			List<String> dataArray = new List<String>();

			
			foreach(String key in response.getData().Keys) {
				if(key.Contains(identifier)) {
					if(base64Decode) {
						dataArray.Add(EncryptionHandler.decodeBase64(response.getField(key)));
					} else {
						dataArray.Add(response.getField(key));	
					}
				}
			}
			
			return dataArray;
		}
		

		//Download a file
		public static Boolean downloadFile(String postfix, String fileName) {
			LogHandler.log(LOG_NAME, "URL: " + serverAddress + postfix);				
			WebClient webClient = new WebClient();
			try {
				//Create the directory that the file will go in if it doesn't already exist
				if(!Directory.Exists(Path.GetDirectoryName(fileName))) {
					Directory.CreateDirectory(Path.GetDirectoryName(fileName));
				}
				webClient.DownloadFile(getServerAddress() + postfix, fileName);

				if(File.Exists(fileName))
					return true;
			} catch (Exception ex) {
				LogHandler.log(LOG_NAME, "Error downloading file");
				LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return false;
		}


		//Get the IP address of the host
		public static String getIPAddress() {
			String hostName = System.Net.Dns.GetHostName();
			
			IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(hostName);
			
			IPAddress[] address = ipEntry.AddressList;
			if(address.Length > 0) //Return the first address listed
				return address[0].ToString();

			return "";
		}

		//Get a string of all mac addresses
		public static String getMacAddresses() {
            String macs = "";
			try {
				NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

				foreach (NetworkInterface adapter in adapters) {
					//Get the mac address for the adapter and add it to the String 'macs', adding ':' as needed
					IPInterfaceProperties properties = adapter.GetIPProperties();

					macs = macs + "|" + string.Join (":", (from z in adapter.GetPhysicalAddress().GetAddressBytes() select z.ToString ("X2")).ToArray());
				}
				
				// Remove the first |
				if(macs.Length > 0)
					macs = macs.Substring(1);
				
			} catch (Exception ex) {
            	LogHandler.log(LOG_NAME, "Error getting MAC addresses");
            	LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);
			}

			return macs;
		}
	}
}