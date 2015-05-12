/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2015 FOG Project
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using OpenSSL.Core;
using RSA = OpenSSL.Crypto.RSA;
// ReSharper disable InconsistentNaming

namespace FOG.Handlers.EncryptionHandler
{
    /// <summary>
    ///     Handle all encryption/decryption
    /// </summary>
    public static class EncryptionHandler
    {
        private const string LOG_NAME = "EncryptionHandler";

        /// <summary>
        ///     Base64 encode a string
        ///     <param name="toEncode">The string that will be encoded</param>
        ///     <returns>A base64 encoded string</returns>
        /// </summary>
        public static string EncodeBase64(string toEncode)
        {
            try
            {
                var bytes = Encoding.ASCII.GetBytes(toEncode);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                LogHandler.LogHandler.Log(LOG_NAME, "Error encoding base64");
                LogHandler.LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
            }
            return "";
        }

        /// <summary>
        ///     Decodes a base64 encoded string
        ///     <param name="toDecode">A base64 encoded string</param>
        ///     <returns>Returns the base64 decoded string</returns>
        /// </summary>
        public static string DecodeBase64(string toDecode)
        {
            try
            {
                var bytes = Convert.FromBase64String(toDecode);
                return Encoding.ASCII.GetString(bytes);
            }
            catch (Exception ex)
            {
                LogHandler.LogHandler.Log(LOG_NAME, "Error decoding base64");
                LogHandler.LogHandler.Log(LOG_NAME, string.Format("ERROR: {0}", ex.Message));
            }
            return "";
        }

        /// <summary>
        ///     AES decrypts a string
        /// </summary>
        /// <param name="toDecode">The string to be decrypted</param>
        /// <param name="key">The AES pass key</param>
        /// <param name="iv">The AES initialization vector</param>
        /// <returns>A decrypted version of the data</returns>
        public static string AESDecrypt(byte[] toDecode, byte[] key, byte[] iv)
        {
            try
            {
                using (var rijndaelManaged = new RijndaelManaged { Key = key, IV = iv, Mode = CipherMode.CBC, Padding = PaddingMode.Zeros})
                using (var memoryStream = new MemoryStream(toDecode))
                using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    //Return the  stream, but trim null bytes due to reading too far
                    return new StreamReader(cryptoStream).ReadToEnd().Replace("\0", string.Empty).Trim();
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogHandler.Log(LOG_NAME, "Error decoding from AES");
                LogHandler.LogHandler.Log(LOG_NAME, string.Format("ERROR: {0}", ex.Message));
            }
            return "";
        }

        /// <summary>
        ///     AES decrypts a string
        ///     <param name="toDecode">The hex-code string to be decrypted</param>
        ///     <param name="passKey">The AES pass key</param>
        ///     <param name="initializationVector">The AES initialization vector</param>
        ///     <returns>An decrypted string of toDecode</returns>
        /// </summary>
        public static string AESDecrypt(string toDecode, string passKey, string initializationVector)
        {
            //Convert the initialization vector and key into a byte array
            var key = Encoding.UTF8.GetBytes(passKey);
            var iv = Encoding.UTF8.GetBytes(initializationVector);
            var msg = HexStringToByteArray(toDecode);

            return AESDecrypt(msg, key, iv);
        }

        /// <summary>
        ///     Securley generates a random number
        ///     <param name="rngProvider">The RNG crypto provider used</param>
        ///     <param name="min">The minimum value of the random number</param>
        ///     <param name="max">The maximum value of the random number</param>
        ///     <returns>A random number between min and max</returns>
        /// </summary>
        public static int GetRandom(RNGCryptoServiceProvider rngProvider, int min, int max)
        {
            var b = new byte[sizeof (uint)];
            rngProvider.GetBytes(b);
            var d = BitConverter.ToUInt32(b, 0)/(double) uint.MaxValue;
            
            return min + (int) ((max - min)*d);
        }

        /// <summary>
        ///     Securely generates an alpha-numerical password
        ///     <param name="length">The number of characters the password should contain</param>
        ///     <returns>A randomly generated password</returns>
        /// </summary>
        public static string GeneratePassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new RNGCryptoServiceProvider();

            for (var i = 0; i < stringChars.Length; i++)
                stringChars[i] = chars[GetRandom(random, 1, chars.Length)];

            return new string(stringChars);
        }

        /// <summary>
        ///     Encrypts a string using RSA
        ///     <param name="toEncode">The string to be encrypted</param>
        ///     <param name="pemFile">The path to a PEM file</param>
        ///     <returns>The encrypted version of toEncode</returns>
        /// </summary>
        public static string RSAEncrypt(string toEncode, string pemFile)
        {
            var byteConverter = new UTF8Encoding();
            var message = byteConverter.GetBytes(toEncode);

            return ByteArrayToHexString(RawRSAEncrypt(message, pemFile));
        }

        /// <summary>
        ///     RSA encrypts a message
        /// </summary>
        /// <param name="toEncode">The message to encrypt</param>
        /// <param name="pemFile">The filepath of the key</param>
        /// <returns>An encrypted hex encoded string of the message</returns>
        public static string RSAEncrypt(byte[] toEncode, string pemFile)
        {
            return ByteArrayToHexString(RawRSAEncrypt(toEncode, pemFile));
        }

        /// <summary>
        ///     RSA encrypts a message
        /// </summary>
        /// <param name="toEncode">The message to encrypt</param>
        /// <param name="pemFile">The filepath of the key</param>
        /// <returns>A encrypted byte array of the message</returns>
        public static byte[] RawRSAEncrypt(byte[] toEncode, string pemFile)
        {
            byte[] encryptedData;
            using (var openSLLRSA = RSA.FromPublicKey(BIO.File(pemFile, "r")))
                encryptedData = openSLLRSA.PublicEncrypt(toEncode, RSA.Padding.PKCS1);

            return encryptedData;
        }

        /// <summary>
        ///     Converts a byte array to a hex string
        ///     <param name="ba">The byte array to be converted</param>
        ///     <returns>A hex string representation of the byte array</returns>
        /// </summary>
        public static string ByteArrayToHexString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length*2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            
            return hex.ToString();
        }

        /// <summary>
        ///     Converts a hex string to a byte array
        ///     <param name="hex">The hex string to be converted</param>
        ///     <returns>A byte array representation of the hex string</returns>
        /// </summary>
        public static byte[] HexStringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars/2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            
            return bytes;
        }

        /// <summary>
        ///     Decrypts a string using AES, and automatically extracts the initialization vector
        ///     <param name="toDecode">The string to be decrypted</param>
        ///     <param name="key">The AES pass key to use</param>
        ///     <returns>A decrypted version of toDecode</returns>
        /// </summary>
        public static string AESDecrypt(string toDecode, byte[] key)
        {
            LogHandler.LogHandler.Log(LOG_NAME, toDecode);
            var iv = HexStringToByteArray(toDecode.Substring(0, toDecode.IndexOf("|", StringComparison.Ordinal)));
            var data = HexStringToByteArray(toDecode.Substring(toDecode.IndexOf("|", StringComparison.Ordinal) + 1));

            return AESDecrypt(data, key, iv);
        }

        /// <summary>
        ///     Creates an md5 hash of a file
        ///     <param name="filePath">The path to the file</param>
        ///     <returns>The md5 hash of the given file</returns>
        /// </summary>
        public static string GetMD5Hash(string filePath)
        {
            if (!File.Exists(filePath)) return "";

            var sBuilder = new StringBuilder();
            var md5 = new MD5CryptoServiceProvider();
            var bytes = File.ReadAllBytes(filePath);
            var result = md5.ComputeHash(bytes);
            foreach (var hashInt in result)
                sBuilder.Append(hashInt.ToString("x2"));

            return sBuilder.ToString();
        }
    }
}