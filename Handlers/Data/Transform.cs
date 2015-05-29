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

// ReSharper disable InconsistentNaming

namespace FOG.Handlers.Data
{
    /// <summary>
    ///     Handle all encryption/decryption
    /// </summary>
    public static class Transform
    {
        private const string LogName = "EncryptionHandler";

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
                Log.Error(LogName, "Could not base64 encode");
                Log.Error(LogName, ex);
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
                Log.Error(LogName, "Could not base64 decode");
                Log.Error(LogName, ex);
            }
            return "";
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
        ///     Creates an md5 hash of bytes
        ///     <param name="data">The bytes to hash</param>
        ///     <returns></returns>
        /// </summary>
        public static string MD5Hash(byte[] data)
        {
            if (data == null) return null;

            var sBuilder = new StringBuilder();
            var md5 = new MD5CryptoServiceProvider();
            var result = md5.ComputeHash(data);
            foreach (var hashInt in result)
                sBuilder.Append(hashInt.ToString("x2"));

            return sBuilder.ToString();
        }

        /// <summary>
        ///     Creates an md5 hash of a file
        ///     <param name="filePath">The path to the file</param>
        ///     <returns></returns>
        /// </summary>
        public static string MD5Hash(string filePath)
        {
            return !File.Exists(filePath) ? null : MD5Hash(File.ReadAllBytes(filePath));
        }
    }
}