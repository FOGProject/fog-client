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

			var messages = new Dictionary<String, String>();
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
		public static void SetServerAddress(String address) { serverAddress = address; }
		public static String GetPassKey() { return passkey; }
		
		public static void SetServerAddress(String HTTPS, String address, String webRoot) { 
			if(HTTPS.Equals("1")) {
				serverAddress = "https://";
			} else {
				serverAddress = "http://";
			}
			serverAddress = serverAddress  + address + webRoot;
		}
		
		public static Boolean GetAndSetServerAddress() {
			if(RegistryHandler.GetSystemSetting("Server") != null && RegistryHandler.GetSystemSetting("WebRoot") != null && 
			   RegistryHandler.GetSystemSetting("Tray") != null && RegistryHandler.GetSystemSetting("HTTPS") != null) {
				
				CommunicationHandler.SetServerAddress(RegistryHandler.GetSystemSetting("HTTPS"), 
				                                      RegistryHandler.GetSystemSetting("Server"), 
				                                      RegistryHandler.GetSystemSetting("WebRoot"));
				return true;
			}
			LogHandler.Log(LOG_NAME, "Regisitry keys are not set");
			return false;
		}
		
		
		public static String GetServerAddress() { return serverAddress; }		

		
		//Return the response form an address
		public static Response GetResponse(String postfix) {

			LogHandler.Log(LOG_NAME, "URL: " + GetServerAddress() + postfix );

			var webClient = new WebClient();
			try {
				String response = webClient.DownloadString(GetServerAddress() + postfix);
				response = decryptAES(response, GetPassKey());
				//See if the return code is known
				Boolean messageFound = false;
				foreach(String returnMessage in returnMessages.Keys) {
					if(response.StartsWith(returnMessage)) {
						messageFound=true;
						LogHandler.Log(LOG_NAME, "Response: " + returnMessages[returnMessage]);
						break;
					}					
				}
				
				if(!messageFound) {
						LogHandler.Log(LOG_NAME, "Unknown Response: " + response.Replace("\n", ""));					
				}
           
				return parseResponse(response);
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Error contacting FOG server");			
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return new Response();
		}		
		
		public static String GetRawResponse(String postfix) {
			//ID the service as the new one
			if(postfix.Contains(".php?")) {
				postfix = postfix + "&newService=1";
			} else {
				postfix = postfix + "?newService=1";
			}
			LogHandler.Log(LOG_NAME, "URL: " + GetServerAddress() + postfix );
			
			var webClient = new WebClient();
			try {
				String response = webClient.DownloadString(GetServerAddress() + postfix);
				return response;
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Error contacting FOG");			
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return "";
			
		}
		
		public static Boolean Authenticate() {
			try {
	
				String keyPath = AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + "public.key";
				DownloadFile("/management/other/ssl/srvpublic.key", keyPath);
				
				passkey = EncryptionHandler.GeneratePassword(32).Trim();
				String encryptedKey = EncryptionHandler.RSAEncrypt(passkey, keyPath);
				
				Response authenticationResponse = 
					GetResponse("/management/index.php?mac=" +  GetMacAddresses() + "&sub=authorize&sym_key=" + encryptedKey);
				
				if(!authenticationResponse.wasError()) {
					LogHandler.Log(LOG_NAME, "Authenticated");	
					return true;
				}
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);					
			}
			
			LogHandler.Log(LOG_NAME, "Failed to authenticate");	
			return false;
		}
		
		private static String decryptAES(String response, String passKey) {
			const String encryptedFlag = "#!en=";
			const String encryptedFlag2 = "#!enkey=";
			
			if(response.StartsWith(encryptedFlag2)) {
				String decryptedResponse = response.Substring(encryptedFlag2.Length);
				response = EncryptionHandler.AESDecrypt(decryptedResponse, passKey);
			}
			if(response.StartsWith(encryptedFlag)) {
				String decryptedResponse = response.Substring(encryptedFlag.Length);
				response = EncryptionHandler.AESDecrypt(decryptedResponse, passKey);
			}			
			
			
			return response;
		}		

		//Contact FOG at a url, used for submitting data
		public static Boolean Contact(String postfix) {
			//ID the service as the new one
			if(postfix.Contains(".php?")) {
				postfix = postfix + "&newService=1";
			} else {
				postfix = postfix + "?newService=1";
			}			
			LogHandler.Log(LOG_NAME,
			               "URL: " + GetServerAddress() + postfix);
			var webClient = new WebClient();			

			try {
				webClient.DownloadString(GetServerAddress() + postfix);
				return true;
				
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Error contacting FOG");		
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
			}
			return false;
		}

		//Parse the recieved data
		private static Response parseResponse(String rawResponse) {
			String[] data = rawResponse.Split('\n'); //Split the response at every new line
			var parsedData = new Dictionary<String, String>();
			var response = new Response();

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
				LogHandler.Log(LOG_NAME, "Error parsing response");
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return response;
		}
		
		//Get an array from a response
		public static List<String> ParseDataArray(Response response, String identifier, Boolean base64Decode) {
			var dataArray = new List<String>();

			foreach(String key in response.getData().Keys) {
				if(key.Contains(identifier)) {
					if(base64Decode) {
						dataArray.Add(EncryptionHandler.DecodeBase64(response.getField(key)));
					} else {
						dataArray.Add(response.getField(key));	
					}
				}
			}
			
			return dataArray;
		}
		

		//Download a file
		public static Boolean DownloadFile(String postfix, String fileName) {
			LogHandler.Log(LOG_NAME, "URL: " + serverAddress + postfix);	
			var webClient = new WebClient();
			try {
				//Create the directory that the file will go in if it doesn't already exist
				if(!Directory.Exists(Path.GetDirectoryName(fileName))) {
					Directory.CreateDirectory(Path.GetDirectoryName(fileName));
				}
				webClient.DownloadFile(GetServerAddress() + postfix, fileName);

				if(File.Exists(fileName))
					return true;
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Error downloading file");
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return false;
		}


		//Get the IP address of the host
		public static String GetIPAddress() {
			String hostName = System.Net.Dns.GetHostName();
			
			IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(hostName);
			
			IPAddress[] address = ipEntry.AddressList;
			if(address.Length > 0) //Return the first address listed
				return address[0].ToString();

			return "";
		}

		//Get a string of all mac addresses
		public static String GetMacAddresses() {
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
            	LogHandler.Log(LOG_NAME, "Error getting MAC addresses");
            	LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
			}

			return macs;
		}
	}
}