
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using OpenSSL.Core;

namespace FOG {
	/// <summary>
	/// Handle all encryption/decryption
	/// </summary>
	public static class EncryptionHandler {
		
		private const String LOG_NAME = "EncryptionHandler";
		
		//Encode a string to base64
		public static String EncodeBase64(String toEncode) {
			try {
				Byte[] bytes = ASCIIEncoding.ASCII.GetBytes(toEncode);
				return System.Convert.ToBase64String(bytes);
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Error encoding base64");
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
			}
			return "";
		}

		
		//Decode a string from base64
		public static String DecodeBase64(String toDecode) {
			try {
				Byte[] bytes = Convert.FromBase64String(toDecode);
				return Encoding.ASCII.GetString(bytes);
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Error decoding base64");
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return "";
		}
		
        public static String EncodeAES(String plainText, String passKey, String ivString) {
 		    
			//Convert the initialization vector and key into a byte array
			byte[] key = Encoding.UTF8.GetBytes(passKey);
		    byte[] iv  = Encoding.UTF8.GetBytes(ivString);   
		    
		    try {
				byte[] encrypted;
	            // Create an RijndaelManaged object 
	            // with the specified key and IV. 
	            using (RijndaelManaged rijndaelManaged = new RijndaelManaged()) {
	                rijndaelManaged.Key = key;
	                rijndaelManaged.IV = iv;
					rijndaelManaged.Mode = CipherMode.CBC;
					rijndaelManaged.Padding = PaddingMode.Zeros;
	                // Create a decrytor to perform the stream transform.
	                using (ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV)) {
		                // Create the streams used for encryption. 
		                using (MemoryStream msEncrypt = new MemoryStream()) {
		                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
		                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
		                            //Write all data to the stream.
		                            swEncrypt.Write(plainText);
		                        }
		                        encrypted = msEncrypt.ToArray();
		                    }
		                }
	                }
	            }
	
	            // Return the encrypted bytes from the memory stream. 
	            return Convert.ToBase64String(encrypted);
		    } catch (Exception ex) {
		        LogHandler.Log(LOG_NAME, "Error encoding AES");
		        LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);		    	
		    }
			return "";	            
        }
		
		//Decode AES256
		public static String DecodeAES(String toDecode, String passKey, String ivString) {
		    //Convert the initialization vector and key into a byte array
			byte[] key = Encoding.UTF8.GetBytes(passKey);
		    byte[] iv  = Encoding.UTF8.GetBytes(ivString);
		    try {
		    	
		    	using(RijndaelManaged rijndaelManaged = new RijndaelManaged()) {
			        rijndaelManaged.Key = key;
			        rijndaelManaged.IV = iv;
			        rijndaelManaged.Mode = CipherMode.CBC;
			        rijndaelManaged.Padding = PaddingMode.Zeros;
			        using(MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(toDecode))) {
			        	using(CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(key, iv), CryptoStreamMode.Read)) {
					        //Return the  stream, but trim null bytes due to reading too far
					        return new StreamReader(cryptoStream).ReadToEnd().Replace("\0", String.Empty).Trim();		        		
			        	}
				       
			        }
		    	}
		        
		        
		    } catch (Exception ex) {
		        LogHandler.Log(LOG_NAME, "Error decoding from AES");
		        LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);		    	
		    }
			return "";
		}
		
		public static int GetRandom(RNGCryptoServiceProvider rngProvider, int min, int max) {
		    byte[] b = new byte[sizeof(UInt32)];
		    rngProvider.GetBytes(b);
		    double d = BitConverter.ToUInt32(b, 0) / (double)UInt32.MaxValue;
		    return min + (int)((max - min) * d);
		}

		public static String GeneratePassword(int length) {
			var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var stringChars = new char[length];
			var random = new RNGCryptoServiceProvider();
			
			for (int i = 0; i < stringChars.Length; i++) {
				stringChars[i] = chars[GetRandom(random, 1, chars.Length)];
			}
			
			return new String(stringChars);
		}
				
		public static String EncodeRSA(String data, String pemFile) {
			
			byte[] encryptedData;
			using(OpenSSL.Crypto.RSA openSLLRSA = OpenSSL.Crypto.RSA.FromPublicKey(BIO.File(pemFile, "r"))) {
				
				UTF8Encoding byteConverter = new UTF8Encoding();
	
				byte[] message = byteConverter.GetBytes(data);
				encryptedData = openSLLRSA.PublicEncrypt(message, OpenSSL.Crypto.RSA.Padding.PKCS1);
	
				
			}
			return ByteArrayToString(encryptedData);
			
		}	

		public static String ByteArrayToString(byte[] ba) {
			StringBuilder hex = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();			
		}
		
		public static byte[] stringToByteArray(String hex) {
		  int NumberChars = hex.Length;
		  byte[] bytes = new byte[NumberChars / 2];
		  for (int i = 0; i < NumberChars; i += 2)
		    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
		  return bytes;
		}		
		

		//Decode an AES256 encrypted response
		public static String DecodeAES(String response, String passKey) {
			//The first set of 15 characters is the initialization vector, the rest is the encrypted message
			if(response.Length > 16) {
				return EncryptionHandler.DecodeAES(response.Substring(16), passKey, response.Substring(0,16)).Trim();
			} else {
				LogHandler.Log(LOG_NAME, "Unable to decrypt response");
				LogHandler.Log(LOG_NAME, "ERROR: Encrypted data is corrupt");
			}
			return "";
		}
		
		//Generate the md5 hash of a file
		public static  String GetMD5Hash(String file) {
			if (File.Exists(file)) {
				StringBuilder sBuilder = new StringBuilder();
				MD5 md5 = new MD5CryptoServiceProvider();
				byte[] bytes = File.ReadAllBytes(file);
				byte[] result = md5.ComputeHash(bytes);
				foreach(int hashInt in result) {
					sBuilder.Append(hashInt.ToString("x2"));
				}
				return sBuilder.ToString();
			}
			return "";
		}	

		public static byte[] StringToByteArray(string hex) {
		    return Enumerable.Range(0, hex.Length)
		                     .Where(x => x % 2 == 0)
		                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
		                     .ToArray();
		}		
		
	}
}