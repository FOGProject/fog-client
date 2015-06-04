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
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// ReSharper disable InconsistentNaming

namespace FOG.Handlers.Middleware
{
    public static class Authentication
    {
        private const string LogName = "Middleware::Authentication";
        private static byte[] Passkey;
        public static byte[] TestPassKey;

        /// <summary>
        ///     Generate a random AES pass key and securely send it to the server
        ///     <returns>True if successfully authenticated</returns>
        /// </summary>
        public static bool HandShake()
        {
            try
            {
                var keyPath = string.Format("{0}tmp\\public.cer", AppDomain.CurrentDomain.BaseDirectory);
                Communication.DownloadFile("/management/other/ssl/srvpublic.crt", keyPath);
                
                var aes = new AesCryptoServiceProvider();
                aes.GenerateKey();
                Passkey = aes.Key;
                var token = GetSecurityToken("token.dat");

                var certificate = new X509Certificate2(keyPath);

                if (!Data.RSA.IsFromCA(Data.RSA.GetCACertificate(), certificate))
                    throw new Exception("Certificate is not from FOG CA");

                Log.Entry(LogName, "Cert OK");

                var enKey = Data.Transform.ByteArrayToHexString(Data.RSA.Encrypt(certificate, Passkey));
                var enToken = Data.Transform.ByteArrayToHexString(Data.RSA.Encrypt(certificate, token));

                var response = Communication.Post("/management/index.php?sub=authorize", string.Format("sym_key={0}&token={1}&mac={2}", enKey, enToken, Configuration.MACAddresses()));
   

                if (!response.Error)
                {
                    Log.Entry(LogName, "Authenticated");
                    SetSecurityToken("token.dat", Data.Transform.HexStringToByteArray(response.GetField("#token")));
                    return true;
                } 
                
                if (response.ReturnCode.Equals("#!ih"))
                    Communication.Contact(string.Format("/service/register.php?hostname={0}", Dns.GetHostName()), true);

            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not authenticate");
                Log.Error(LogName, ex);
            }

            return false;
        }

        private static byte[] GetSecurityToken(string filePath)
        {
            try
            {
                var token = File.ReadAllBytes(filePath);
                token = Data.DPAPI.UnProtectData(token, true);
                return token;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not get security token");
                Log.Error(LogName, ex);
            }

            return new byte[0];
        }

        private static void SetSecurityToken(string filePath, byte[] token)
        {
            try
            {
                token = Data.DPAPI.ProtectData(token, true);
                File.WriteAllBytes(filePath, token);    
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not save security token");
                Log.Error(LogName, ex);
            }
        }

        /// <summary>
        ///     Decrypts a response using AES, filtering out encryption flags
        ///     <param name="toDecode">The string to decrypt</param>
        ///     <param name="passKey">The AES pass key to use</param>
        ///     <returns>True if the server was contacted successfully</returns>
        /// </summary>
        public static string Decrypt(string toDecode)
        {
            const string encryptedFlag = "#!en=";
            const string encryptedFlag2 = "#!enkey=";

            if (toDecode.StartsWith(encryptedFlag2))
            {
                var decryptedResponse = toDecode.Substring(encryptedFlag2.Length);
                toDecode = Data.AES.Decrypt(decryptedResponse, TestPassKey ?? Passkey);
                return toDecode;
            }
            if (!toDecode.StartsWith(encryptedFlag)) return toDecode;

            var decrypted = toDecode.Substring(encryptedFlag.Length);
            return Data.AES.Decrypt(decrypted, TestPassKey ?? Passkey);
        }
    }
}