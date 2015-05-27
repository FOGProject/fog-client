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

namespace FOG.Handlers.Data
{
    public static class AES
    {
        private const string LogName = "EncryptionHandler";

        /// <summary>
        ///     AES decrypts a string
        /// </summary>
        /// <param name="toDecode">The string to be decrypted</param>
        /// <param name="key">The AES pass key</param>
        /// <param name="iv">The AES initialization vector</param>
        /// <returns>A decrypted version of the data</returns>
        public static string Decrypt(byte[] toDecode, byte[] key, byte[] iv)
        {
            try
            {
                using (var rijndaelManaged = new RijndaelManaged { Key = key, IV = iv, Mode = CipherMode.CBC, Padding = PaddingMode.Zeros })
                using (var memoryStream = new MemoryStream(toDecode))
                using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    //Return the  stream, but trim null bytes due to reading too far
                    return new StreamReader(cryptoStream).ReadToEnd().Replace("\0", string.Empty).Trim();
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not decrypt AES");
                Log.Error(LogName, ex);
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
        public static string Decrypt(string toDecode, string passKey, string initializationVector)
        {
            //Convert the initialization vector and key into a byte array
            var key = Encoding.UTF8.GetBytes(passKey);
            var iv = Encoding.UTF8.GetBytes(initializationVector);
            var msg = Transform.HexStringToByteArray(toDecode);

            return Decrypt(msg, key, iv);
        }

        /// <summary>
        ///     Decrypts a string using AES, and automatically extracts the initialization vector
        ///     <param name="toDecode">The string to be decrypted</param>
        ///     <param name="key">The AES pass key to use</param>
        ///     <returns>A decrypted version of toDecode</returns>
        /// </summary>
        public static string Decrypt(string toDecode, byte[] key)
        {
            Log.Entry(LogName, toDecode);
            var iv = Transform.HexStringToByteArray(toDecode.Substring(0, toDecode.IndexOf("|")));
            var data = Transform.HexStringToByteArray(toDecode.Substring(toDecode.IndexOf("|") + 1));

            return Decrypt(data, key, iv);
        }
    }
}
