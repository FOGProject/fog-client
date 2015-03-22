
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using OpenSSL.Core;

namespace FOG
{
    /// <summary>
    /// Handle all encryption/decryption
    /// </summary>
    public static class EncryptionHandler
    {
		
        private const String LOG_NAME = "EncryptionHandler";
		
        /// <summary>
        /// Base64 encode a string
        /// <param name="toEncode">The string that will be encoded</param>
        /// <returns>A base64 encoded string</returns>
        /// </summary>
        public static String EncodeBase64(String toEncode)
        {
            try
            {
                var bytes = ASCIIEncoding.ASCII.GetBytes(toEncode);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
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
        public static String DecodeBase64(String toDecode)
        {
            try
            {
                var bytes = Convert.FromBase64String(toDecode);
                return Encoding.ASCII.GetString(bytes);
            }
            catch (Exception ex)
            {
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
        public static String AESEncrypt(String toEncode, String passKey, String initializationVector)
        {
            //Convert the initialization vector and key into a byte array
            var key = Encoding.UTF8.GetBytes(passKey);
            var iv = Encoding.UTF8.GetBytes(initializationVector);   

            return AESEncrypt(toEncode, key, iv);        
        }
		
        /// <summary>
        /// AES encrypts a string
        /// </summary>
        /// <param name="toEncode">The string to be encrypted</param>
        /// <param name="key">The AES pass key</param>
        /// <param name="iv">The AES initialization vector</param>
        /// <returns>An encrypted string of toEncode</returns>
        public static String AESEncrypt(String toEncode, byte[] key, byte[] iv)
        {
            try
            {
                byte[] encrypted;
                // Create an RijndaelManaged object 
                // with the specified key and IV. 
                using (var rijndaelManaged = new RijndaelManaged())
                {
                    rijndaelManaged.Key = key;
                    rijndaelManaged.IV = iv;
                    rijndaelManaged.Mode = CipherMode.CBC;
                    rijndaelManaged.Padding = PaddingMode.Zeros;
                    // Create a decrytor to perform the stream transform.
                    using (ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV))
                    {
                        // Create the streams used for encryption. 
                        using (var msEncrypt = new MemoryStream())
                        {
                            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                            {
                                using (var swEncrypt = new StreamWriter(csEncrypt))
                                {
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
            }
            catch (Exception ex)
            {
                LogHandler.Log(LOG_NAME, "Error encoding AES");
                LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);		    	
            }
            return "";	            
        }
        
        /// <summary>
        /// AES decrypts a string
        /// </summary>
        /// <param name="toDecode">The string to be decrypted</param>
        /// <param name="key">The AES pass key</param>
        /// <param name="iv">The AES initialization vector</param>
        /// <returns>A decrypted version of the data</returns>		
        public static String AESDecrypt(byte[] toDecode, byte[] key, byte[] iv)
        {
            try
            {
                using (var rijndaelManaged = new RijndaelManaged())
                {
                    rijndaelManaged.Key = key;
                    rijndaelManaged.IV = iv;
                    rijndaelManaged.Mode = CipherMode.CBC;
                    rijndaelManaged.Padding = PaddingMode.Zeros;
                    using (var memoryStream = new MemoryStream(toDecode))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        {
                            //Return the  stream, but trim null bytes due to reading too far
                            return new StreamReader(cryptoStream).ReadToEnd().Replace("\0", String.Empty).Trim();		        		
                        }
				       
                    }
                }
		        
		        
            }
            catch (Exception ex)
            {
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
        public static int GetRandom(RNGCryptoServiceProvider rngProvider, int min, int max)
        {
            var b = new byte[sizeof(UInt32)];
            rngProvider.GetBytes(b);
            var d = BitConverter.ToUInt32(b, 0) / (double)UInt32.MaxValue;
            return min + (int)((max - min) * d);
        }

        /// <summary>
        /// Securely generates an alpha-numerical password
        /// <param name="length">The number of characters the password should contain</param>
        /// <returns>A randomly generated password</returns>
        /// </summary>
        public static String GeneratePassword(int length)
        {
            const String chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new RNGCryptoServiceProvider();
			
            for (int i = 0; i < stringChars.Length; i++)
            {
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
        public static String RSAEncrypt(String toEncode, String pemFile)
        {
            var byteConverter = new UTF8Encoding();
            var message = byteConverter.GetBytes(toEncode);
			
            return ByteArrayToHexString(RawRSAEncrypt(message, pemFile));
        }
		
        /// <summary>
        /// RSA encrypts a message
        /// </summary>
        /// <param name="toEncode">The message to encrypt</param>
        /// <param name="pemFile">The filepath of the key</param>
        /// <returns>An encrypted hex encoded string of the message</returns>
        public static String RSAEncrypt(byte[] toEncode, String pemFile)
        {
            return ByteArrayToHexString(RawRSAEncrypt(toEncode, pemFile));
        }
        
        /// <summary>
        /// RSA encrypts a message
        /// </summary>
        /// <param name="toEncode">The message to encrypt</param>
        /// <param name="pemFile">The filepath of the key</param>
        /// <returns>A encrypted byte array of the message</returns>		
        public static byte[] RawRSAEncrypt(byte[] toEncode, String pemFile)
        {
            byte[] encryptedData;
            using (OpenSSL.Crypto.RSA openSLLRSA = OpenSSL.Crypto.RSA.FromPublicKey(BIO.File(pemFile, "r")))
            {
                encryptedData = openSLLRSA.PublicEncrypt(toEncode, OpenSSL.Crypto.RSA.Padding.PKCS1);
            }
            return encryptedData;
        }
				
        /// <summary>
        /// RSA decrypts a message
        /// </summary>
        /// <param name="toDecode">The message to decrypt</param>
        /// <param name="rsa">The rsa object storing the key info</param>
        /// <returns>A decrypted string of the message</returns>			
        public static String RSADecrypt(String toDecode, OpenSSL.Crypto.RSA rsa)
        {
            return Encoding.Default.GetString(RawRSADecrypt(toDecode, rsa));
        }
        
        /// <summary>
        /// RSA decrypts a message
        /// </summary>
        /// <param name="toDecode">The message to decrypt</param>
        /// <param name="rsa">The rsa object storing the key info</param>
        /// <returns>A decrypted byte array of the message</returns>		
        public static byte[] RawRSADecrypt(String toDecode, OpenSSL.Crypto.RSA rsa)
        {
            var message = HexStringToByteArray(toDecode);
            var encryptedData = rsa.PrivateDecrypt(message, OpenSSL.Crypto.RSA.Padding.PKCS1);

            return encryptedData;
        }
		
        /// <summary>
        /// Converts a byte array to a hex string
        /// <param name="ba">The byte array to be converted</param>
        /// <returns>A hex string representation of the byte array</returns>
        /// </summary>
        public static String ByteArrayToHexString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();			
        }
		
        /// <summary>
        /// Converts a hex string to a byte array
        /// <param name="hex">The hex string to be converted</param>
        /// <returns>A byte array representation of the hex string</returns>
        /// </summary>			
        public static byte[] HexStringToByteArray(String hex)
        {
            var NumberChars = hex.Length;
            var bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
		

        /// <summary>
        /// Decrypts a string using AES, and automatically extracts the initialization vector
        /// <param name="toDecode">The string to be decrypted</param>
        /// <param name="key">The AES pass key to use</param>
        /// <returns>A decrypted version of toDecode</returns>
        /// </summary>	
        public static String AESDecrypt(String toDecode, byte[] key)
        {
            var iv = EncryptionHandler.HexStringToByteArray(toDecode.Substring(0, toDecode.IndexOf("|")));
            var data = EncryptionHandler.HexStringToByteArray(toDecode.Substring(toDecode.IndexOf("|") + 1));
			
            return EncryptionHandler.AESDecrypt(data, key, iv);
        }
		
        /// <summary>
        /// Creates an md5 hash of a file
        /// <param name="filePath">The path to the file</param>
        /// <returns>The md5 hash of the given file</returns>
        /// </summary>	
        public static String GetMD5Hash(String filePath)
        {
            if (File.Exists(filePath))
            {
                var sBuilder = new StringBuilder();
                var md5 = new MD5CryptoServiceProvider();
                var bytes = File.ReadAllBytes(filePath);
                var result = md5.ComputeHash(bytes);
                foreach (int hashInt in result)
                {
                    sBuilder.Append(hashInt.ToString("x2"));
                }
                return sBuilder.ToString();
            }
            return "";
        }
		
    }
}