
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
		
		/// <summary>
		/// Base64 encode a string
		/// <param name="toEncode">The string that will be encoded</param>
		/// <returns>A base64 encoded string</returns>
		/// </summary>
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
		
		/// <summary>
		/// Decodes a base64 encoded string
		/// <param name="toDecode">A base64 encoded string</param>
		/// <returns>Returns the base64 decoded string</returns>
		/// </summary>
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
		
		/// <summary>
		/// AES encrypts a string
		/// <param name="toEncode">The string to be encrypted</param>
		/// <param name="passKey">The AES pass key</param>
		/// <param name="initializationVector">The AES initialization vector</param>
		/// <returns>An encrypted string of toEncode</returns>
		/// </summary>
        public static String AESEncrypt(String toEncode, String passKey, String initializationVector) {
 		    
			//Convert the initialization vector and key into a byte array
			byte[] key = Encoding.UTF8.GetBytes(passKey);
		    byte[] iv  = Encoding.UTF8.GetBytes(initializationVector);   

		    return AESEncrypt(toEncode, key, iv);
		    	            
        }
		
		public static String AESEncrypt(String toEncode, byte[] key, byte[] iv) {
		    
		    try {
				byte[] encrypted;
	            // Create an RijndaelManaged object 
	            // with the specified key and IV. 
	            using (RijndaelManaged rijndaelManaged = new RijndaelManaged()) {
	                rijndaelManaged.Key = key;
	                rijndaelManaged.IV = iv;
					rijndaelManaged.Mode = CipherMode.CBC;
					rijndaelManaged.Padding = PaddingMode.PKCS7;
	                // Create a decrytor to perform the stream transform.
	                using (ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV)) {
		                // Create the streams used for encryption. 
		                using (MemoryStream msEncrypt = new MemoryStream()) {
		                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
		                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
		                            //Write all data to the stream.
		                            swEncrypt.Write(toEncode);
		                        }
		                        encrypted = msEncrypt.ToArray();
		                    }
		                }
	                }
	            }
	
	            // Return the encrypted bytes from the memory stream. 
	            return ByteArrayToHexString(encrypted);
		    } catch (Exception ex) {
		        LogHandler.Log(LOG_NAME, "Error encoding AES");
		        LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);		    	
		    }
			return "";	            
        }		
		
		/// <summary>
		/// AES decrypts a string
		/// <param name="toDecode">The string to be decrypted</param>
		/// <param name="passKey">The AES pass key</param>
		/// <param name="initializationVector">The AES initialization vector</param>
		/// <returns>A decrypted string of toDecode</returns>
		/// </summary>
		public static String AESDecrypt(String toDecode, String passKey, String initializationVector) {

		    //Convert the initialization vector and key into a byte array
			byte[] key = Encoding.UTF8.GetBytes(passKey);
		    byte[] iv  = Encoding.UTF8.GetBytes(initializationVector);
		    byte[] msg = HexStringToByteArray(toDecode);
		    return AESDecrypt(msg, key, iv);
		}
		
		public static String AESDecrypt(byte[] toDecode, byte[] key, byte[] iv) {
		    try {
		    	
		    	using(RijndaelManaged rijndaelManaged = new RijndaelManaged()) {
			        rijndaelManaged.Key = key;
			        rijndaelManaged.IV = iv;
			        rijndaelManaged.Mode = CipherMode.CBC;
			        rijndaelManaged.Padding = PaddingMode.PKCS7;
			        using(MemoryStream memoryStream = new MemoryStream(toDecode)) {
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
		
		/// <summary>
		/// Securley generates a random number
		/// <param name="rngProvider">The RNG crypto provider used</param>
		/// <param name="min">The minimum value of the random number</param>
		/// <param name="max">The maximum value of the random number</param>
		/// <returns>A random number between min and max</returns>
		/// </summary>		
		public static int GetRandom(RNGCryptoServiceProvider rngProvider, int min, int max) {
		    byte[] b = new byte[sizeof(UInt32)];
		    rngProvider.GetBytes(b);
		    double d = BitConverter.ToUInt32(b, 0) / (double)UInt32.MaxValue;
		    return min + (int)((max - min) * d);
		}

		/// <summary>
		/// Securely generates an alpha-numerical password
		/// <param name="length">The number of characters the password should contain</param>
		/// <returns>A randomly generated password</returns>
		/// </summary>
		public static String GeneratePassword(int length) {
			var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var stringChars = new char[length];
			var random = new RNGCryptoServiceProvider();
			
			for (int i = 0; i < stringChars.Length; i++) {
				stringChars[i] = chars[GetRandom(random, 1, chars.Length)];
			}
			
			return new String(stringChars);
		}		
		
		/// <summary>
		/// Encrypts a string using RSA
		/// <param name="toEncode">The string to be encrypted</param>
		/// <param name="pemFile">The path to a PEM file</param>
		/// <returns>The encrypted version of toEncode</returns>
		/// </summary>		
		public static String RSAEncrypt(String toEncode, String pemFile) {
			return ByteArrayToHexString(RawRSAEncrypt(toEncode, pemFile));
		}
		
		public static byte[] RawRSAEncrypt(String toEncode, String pemFile) {
			
			byte[] encryptedData;
			using(OpenSSL.Crypto.RSA openSLLRSA = OpenSSL.Crypto.RSA.FromPublicKey(BIO.File(pemFile, "r"))) {
				
				UTF8Encoding byteConverter = new UTF8Encoding();
	
				byte[] message = byteConverter.GetBytes(toEncode);

				encryptedData = openSLLRSA.PublicEncrypt(message, OpenSSL.Crypto.RSA.Padding.PKCS1);
				
			}
			return encryptedData;
			
		}
				
		
		public static String RSADecrypt(String toDecode, OpenSSL.Crypto.RSA rsa) {
			return Encoding.Default.GetString(RawRSADecrypt(toDecode, rsa));
		}

		public static byte[] RawRSADecrypt(String toDecode, OpenSSL.Crypto.RSA rsa) {
	
			byte[] message = HexStringToByteArray(toDecode);
			var encryptedData = rsa.PrivateDecrypt(message, OpenSSL.Crypto.RSA.Padding.PKCS1);

			
			return encryptedData;
			
		}		
		
		/// <summary>
		/// Converts a byte array to a hex string
		/// <param name="ba">The byte array to be converted</param>
		/// <returns>A hex string representation of the byte array</returns>
		/// </summary>	
		public static String ByteArrayToHexString(byte[] ba) {
			StringBuilder hex = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();			
		}
		
		/// <summary>
		/// Converts a hex string to a byte array
		/// <param name="hex">The hex string to be converted</param>
		/// <returns>A byte array representation of the hex string</returns>
		/// </summary>			
		public static byte[] HexStringToByteArray(String hex) {
		  int NumberChars = hex.Length;
		  byte[] bytes = new byte[NumberChars / 2];
		  for (int i = 0; i < NumberChars; i += 2)
		    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
		  return bytes;
		}		
		

		/// <summary>
		/// Decrypts a string using AES, and automatically extracts the initialization vector
		/// <param name="toDecode">The string to be decrypted</param>
		/// <param name="passKey">The AES pass key to use</param>
		/// <returns>A decrypted version of toDecode</returns>
		/// </summary>	
		public static String AESDecrypt(String toDecode, String passKey) {
			//The first set of 15 characters is the initialization vector, the rest is the encrypted message
			if(toDecode.Length > 16) {
				String iv = toDecode.Substring(16);
				toDecode = toDecode.Substring(0,16).Trim();
				return EncryptionHandler.AESDecrypt(iv, passKey, toDecode);
			} else {
				LogHandler.Log(LOG_NAME, "Unable to decrypt response");
				LogHandler.Log(LOG_NAME, "ERROR: Encrypted data is corrupt");
			}
			return "";
		}
		
		/// <summary>
		/// Creates an md5 hash of a file
		/// <param name="filePath">The path to the file</param>
		/// <returns>The md5 hash of the given file</returns>
		/// </summary>	
		public static  String GetMD5Hash(String filePath) {
			if (File.Exists(filePath)) {
				StringBuilder sBuilder = new StringBuilder();
				MD5 md5 = new MD5CryptoServiceProvider();
				byte[] bytes = File.ReadAllBytes(filePath);
				byte[] result = md5.ComputeHash(bytes);
				foreach(int hashInt in result) {
					sBuilder.Append(hashInt.ToString("x2"));
				}
				return sBuilder.ToString();
			}
			return "";
		}		
		
	}
}