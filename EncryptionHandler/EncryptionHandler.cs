
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security;
using System.Linq;
using System.Xml.Linq;

namespace FOG {
	/// <summary>
	/// Handle all encryption/decryption
	/// </summary>
	public static class EncryptionHandler {
		
		private const String LOG_NAME = "EncryptionHandler";
		private static RSACryptoServiceProvider rsa;
		private static RSACryptoServiceProvider serverRSA = new RSACryptoServiceProvider();
		
		//Encode a string to base64
		public static String encodeBase64(String toEncode) {
			try {
				Byte[] bytes = ASCIIEncoding.ASCII.GetBytes(toEncode);
				return System.Convert.ToBase64String(bytes);
			} catch (Exception ex) {
				LogHandler.log(LOG_NAME, "Error encoding base64");
				LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return "";
		}

		
		//Decode a string from base64
		public static String decodeBase64(String toDecode) {
			try {
				Byte[] bytes = Convert.FromBase64String(toDecode);
				return Encoding.ASCII.GetString(bytes);
			} catch (Exception ex) {
				LogHandler.log(LOG_NAME, "Error decoding base64");
				LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);				
			}
			return "";
		}
		
		//Decode AES256
		private static String decodeAES(String toDecode, String passKey, String ivString) {
		    //Convert the initialization vector and key into a byte array
			byte[] key = Encoding.UTF8.GetBytes(passKey);
		    byte[] iv  = Encoding.UTF8.GetBytes(ivString);
		
		    try {
		    	
		        RijndaelManaged rijndaelManaged = new RijndaelManaged();
		        rijndaelManaged.Key = key;
		        rijndaelManaged.IV = iv;
		        rijndaelManaged.Mode = CipherMode.CBC;
		        rijndaelManaged.Padding = PaddingMode.Zeros;
		        MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(toDecode));
		        CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(key, iv), CryptoStreamMode.Read);
		        
		        //Return the  stream, but trim null bytes due to reading too far
		        return new StreamReader(cryptoStream).ReadToEnd().Replace("\0", String.Empty).Trim();
		        
		        
		    } catch (Exception ex) {
		        LogHandler.log(LOG_NAME, "Error decoding from AES");
		        LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);		    	
		    }
			return "";
		}
		
		public static String decodeRSA(RSACryptoServiceProvider rsa, String data) {
			ASCIIEncoding byteConverter = new ASCIIEncoding();
			
			byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(data), false);
			
			return byteConverter.GetString(decryptedData);
		}
		
		public static String encodeRSA(RSACryptoServiceProvider rsa, String toEncode) {
			ASCIIEncoding byteConverter = new ASCIIEncoding();
			
			byte[] dataToEncrypt = byteConverter.GetBytes(toEncode);
			byte[] encryptedData = rsa.Encrypt(dataToEncrypt, false);
			
			String encryptedDataString = Convert.ToBase64String(encryptedData);
			
			return encryptedDataString;
		}
		
		public static void generateRSA() {
			rsa = new RSACryptoServiceProvider(2048);
		}
		
		public static String getRSAPublicKey() {
			RSAParameters rsaParams = rsa.ExportParameters(false);
			ASCIIEncoding byteConverter = new ASCIIEncoding();
			
			String modulus = byteConverter.GetString(rsaParams.Modulus);
			String exponent = byteConverter.GetString(rsaParams.Exponent);
				
			return modulus + exponent;
		}
		
		public static void setServerRSA(String publicKey) {
			RSAParameters rsaParams = new RSAParameters();
			ASCIIEncoding byteConverter = new ASCIIEncoding();
		
			rsaParams.Modulus = byteConverter.GetBytes(publicKey.Substring(0, publicKey.Length-4));
			LogHandler.log(LOG_NAME, "Modulus= " + byteConverter.GetString(rsaParams.Modulus));
			rsaParams.Exponent = byteConverter.GetBytes(publicKey.Substring(publicKey.Length-4, 4));
			LogHandler.log(LOG_NAME, "Exponent= " + byteConverter.GetString(rsaParams.Exponent));
			rsa.exp
			serverRSA.ImportParameters(rsaParams);
		}
		
		public static RSACryptoServiceProvider getServerRSA() {
			return serverRSA;
		}
		
		public static RSACryptoServiceProvider getClientRSA() {
			return rsa;
		}
		
		
		

		//Decode an AES256 encrypted response
		public static String decodeAES(String response, String passKey) {
			//The first set of 15 characters is the initialization vector, the rest is the encrypted message
			if(response.Length > 16) {
				return EncryptionHandler.decodeAES(response.Substring(16), passKey, response.Substring(0,16)).Trim();
			} else {
				LogHandler.log(LOG_NAME, "Unable to decrypt response");
				LogHandler.log(LOG_NAME, "ERROR: Encrypted data is corrupt");
			}
			return "";
		}
		
		//Generate the md5 hash of a file
		public static  String generateMD5Hash(String file) {
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