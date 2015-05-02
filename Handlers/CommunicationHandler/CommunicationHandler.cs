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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
// ReSharper disable InconsistentNaming

namespace FOG.Handlers
{
    /// <summary>
    ///     Handle all communication with the FOG Server
    /// </summary>
    public static class CommunicationHandler
    {
        private const string SuccessCode = "#!ok";
        private const string LogName = "CommunicationHandler";
        private static readonly Dictionary<string, string> ReturnMessages = new Dictionary<string, string>
        {
            {SuccessCode, "Success"},
            {"#!db", "Database error"},
            {"#!im", "Invalid MAC address format"},
            {"#!ihc", "Invalid host certificate"},
            {"#!ih", "Invalid host"},
            {"#!il", "Invalid login"},
            {"#!it", "Invalid task"},
            {"#!ng", "Module is disabled globally on the FOG server"},
            {"#!nh", "Module is disabled on the host"},
            {"#!um", "Unknown module ID"},
            {"#!ns", "No snapins"},
            {"#!nj", "No jobs"},
            {"#!na", "No actions"},
            {"#!nf", "No updates"},
            {"#!time", "Invalid time"},
            {"#!er", "General error"}
        };

        //Define variables
        public static string ServerAddress { get; set; }
        private static byte[] Passkey { get; set; }

        private static bool _isAddressSet = GetAndSetServerAddress();



        /// <summary>
        ///     Load the server information from the registry and apply it
        ///     <returns>True if settings were updated</returns>
        /// </summary>
        public static bool GetAndSetServerAddress()
        {
            if (RegistryHandler.GetSystemSetting("Server") != null &&
                RegistryHandler.GetSystemSetting("WebRoot") != null &&
                RegistryHandler.GetSystemSetting("Tray") != null && RegistryHandler.GetSystemSetting("HTTPS") != null)
            {
                ServerAddress = (RegistryHandler.GetSystemSetting("HTTPS").Equals("1") ? "https://" : "http://");
                ServerAddress += RegistryHandler.GetSystemSetting("Server") +
                                 RegistryHandler.GetSystemSetting("WebRoot");
                return true;
            }
            LogHandler.Log(LogName, "Regisitry keys are not set");

            return false;
        }

        /// <summary>
        ///     Get the parsed response of a server url
        ///     <param name="postfix">The postfix to attach to the server address</param>
        ///     <returns>The parsed response</returns>
        /// </summary>
        public static Response GetResponse(string postfix)
        {
            //ID the service as the new one
            postfix += ((postfix.Contains(".php?") ? "&" : "?") + "newService=1");

            LogHandler.Log(LogName, "URL: " + ServerAddress + postfix);

            var webClient = new WebClient();
            try
            {
                var response = webClient.DownloadString(ServerAddress + postfix);
                response = AESDecrypt(response, Passkey);
                //See if the return code is known
                var messageFound = false;
                foreach (var returnMessage in ReturnMessages.Keys.Where(returnMessage => response.StartsWith(returnMessage)))
                {
                    messageFound = true;
                    LogHandler.Log(LogName, "Response: " + ReturnMessages[returnMessage]);
                    break;
                }

                if (!messageFound)
                    LogHandler.Log(LogName, "Unknown Response: " + response.Replace("\n", ""));

                return parseResponse(response);
            }
            catch (Exception ex)
            {
                LogHandler.Log(LogName, "Error contacting FOG server");
                LogHandler.Log(LogName, "ERROR: " + ex.Message);
            }
           
            return new Response();
        }

        /// <summary>
        ///     Get the parsed response of a server url
        /// </summary>
        /// <param name="postfix">The postfix to attach to the server address</param>
        /// <param name="appendMAC">If the MAC address of the host should be appended to the URL</param>
        /// <returns>The parsed response</returns>
        public static Response GetResponse(string postfix, bool appendMAC)
        {
            if (appendMAC)
                postfix += ((postfix.Contains(".php?") ? "&" : "?") + "mac=" + GetMacAddresses());
           
            return GetResponse(postfix);
        }

        /// <summary>
        ///     Get the raw response of a server url
        ///     <param name="postfix">The postfix to attach to the server address</param>
        ///     <returns>The unparsed response</returns>
        /// </summary>
        public static string GetRawResponse(string postfix)
        {
            //ID the service as the new one
            postfix += ((postfix.Contains(".php?") ? "&" : "?") + "newService=1");

            LogHandler.Log(LogName, "URL: " + ServerAddress + postfix);

            var webClient = new WebClient();

            try
            {
                var response = webClient.DownloadString(ServerAddress + postfix);
                return response;
            }
            catch (Exception ex)
            {
                LogHandler.Log(LogName, "Error contacting FOG");
                LogHandler.Log(LogName, "ERROR: " + ex.Message);
            }

            return "";
        }

        /// <summary>
        ///     Generate a random AES pass key and securely send it to the server
        ///     <returns>True if successfully authenticated</returns>
        /// </summary>
        public static bool Authenticate()
        {
            try
            {
                var keyPath = AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + "public.key";
                DownloadFile("/management/other/ssl/srvpublic.key", keyPath);
                var aes = new AesCryptoServiceProvider();
                aes.GenerateKey();

                Passkey = aes.Key;

                var encryptedKey = EncryptionHandler.RSAEncrypt(Passkey, keyPath);
                var authenticationResponse = GetResponse("/management/index.php?sub=authorize&sym_key=" + encryptedKey,
                    true);

                if (!authenticationResponse.Error)
                {
                    LogHandler.Log(LogName, "Authenticated");
                    return true;
                }

                if (authenticationResponse.ReturnCode.Equals("#!ih"))
                    Contact("/service/register.php?hostname=" + Dns.GetHostName(), true);
            }
            catch (Exception ex)
            {
                LogHandler.Log(LogName, "ERROR: " + ex.Message);
            }

            LogHandler.Log(LogName, "Failed to authenticate");
            return false;
        }

        /// <summary>
        ///     Decrypts a response using AES, filtering out encryption flags
        ///     <param name="toDecode">The string to decrypt</param>
        ///     <param name="passKey">The AES pass key to use</param>
        ///     <returns>True if the server was contacted successfully</returns>
        /// </summary>
        private static string AESDecrypt(string toDecode, byte[] passKey)
        {
            const string encryptedFlag = "#!en=";
            const string encryptedFlag2 = "#!enkey=";

            if (toDecode.StartsWith(encryptedFlag2))
            {
                var decryptedResponse = toDecode.Substring(encryptedFlag2.Length);
                toDecode = EncryptionHandler.AESDecrypt(decryptedResponse, passKey);
                return toDecode;
            }
            if (toDecode.StartsWith(encryptedFlag))
            {
                var decryptedResponse = toDecode.Substring(encryptedFlag.Length);
                toDecode = EncryptionHandler.AESDecrypt(decryptedResponse, passKey);
            }


            return toDecode;
        }

        /// <summary>
        ///     Notify the server of something but don't check for a response
        ///     <param name="postfix">The postfix to attach to the server address</param>
        ///     <returns>True if the server was contacted successfully</returns>
        /// </summary>
        public static bool Contact(string postfix)
        {
            //ID the service as the new one
            postfix += ((postfix.Contains(".php?") ? "&" : "?") + "newService=1");

            LogHandler.Log(LogName,
                "URL: " + ServerAddress + postfix);
            var webClient = new WebClient();

            try
            {
                webClient.DownloadString(ServerAddress + postfix);
                return true;
            }
            catch (Exception ex)
            {
                LogHandler.Log(LogName, "Error contacting FOG");
                LogHandler.Log(LogName, "ERROR: " + ex.Message);
            }
            return false;
        }

        public static bool Contact(string postfix, bool appendMAC)
        {
            if (appendMAC)
                postfix += ((postfix.Contains(".php?") ? "&" : "?") + "mac=" + GetMacAddresses());

            return Contact(postfix);
        }

        /// <summary>
        ///     Parse a raw response for all objects
        ///     <param name="rawResponse">The unparsed response</param>
        ///     <returns>A response object containing all of the parsed information</returns>
        /// </summary>
        private static Response parseResponse(string rawResponse)
        {
            var data = rawResponse.Split('\n'); //Split the response at every new line
            var parsedData = new Dictionary<string, string>();
            var response = new Response();

            try
            {
                //Get and set the error boolean
                var returnCode = data[0];
                response.ReturnCode = returnCode;
                response.Error = !returnCode.ToLower().Trim().StartsWith(SuccessCode);

                //Loop through each line returned and if it contains an '=' add it to the dictionary
                foreach (var element in data.Where(element => element.Contains("=")))
                {
                    parsedData.Add(element.Substring(0, element.IndexOf("=", StringComparison.Ordinal)).Trim(),
                        element.Substring(element.IndexOf("=", StringComparison.Ordinal) + 1).Trim());
                }

                response.Data = parsedData;
            }
            catch (Exception ex)
            {
                LogHandler.Log(LogName, "Error parsing response");
                LogHandler.Log(LogName, "ERROR: " + ex.Message);
            }
            return response;
        }

        /// <summary>
        ///     Parse a Response for an array of objects
        ///     <param name="response">The response to parse</param>
        ///     <param name="identifier">The string identifier infront of the elements</param>
        ///     <param name="base64Decode">Whether the elements should be base64 decoded</param>
        ///     <returns>A List of the elements matching the identifier</returns>
        /// </summary>
        public static List<string> ParseDataArray(Response response, string identifier, bool base64Decode)
        {
            return response.Data.Keys.Where(key => key.Contains(identifier)).Select(key => 
                base64Decode 
                ? EncryptionHandler.DecodeBase64(response.GetField(key)) 
                : response.GetField(key))
                .ToList();
        }

        /// <summary>
        ///     Downloads a file and creates necessary directories
        ///     <param name="postfix">The postfix to attach to the server address</param>
        ///     <param name="filePath">The location to save the file</param>
        ///     <returns>True if the download was successful</returns>
        /// </summary>
        public static bool DownloadFile(string postfix, string filePath)
        {
            return DownloadExternalFile(ServerAddress + postfix, filePath);
        }

        public static bool DownloadExternalFile(string url, string filePath)
        {
            LogHandler.Log(LogName, "URL: " + url);
            var webClient = new WebClient();
            try
            {
                //Create the directory that the file will go in if it doesn't already exist
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                webClient.DownloadFile(url, filePath);

                if (File.Exists(filePath))
                    return true;
            }
            catch (Exception ex)
            {
                LogHandler.Log(LogName, "Error downloading file");
                LogHandler.Log(LogName, "ERROR: " + ex.Message);
            }
            return false;
        }

        /// <summary>
        ///     Get the IP address of the host
        ///     <returns>The first IP address of the host</returns>
        /// </summary>
        public static string GetIPAddress()
        {
            var hostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(hostName);
            var address = ipEntry.AddressList;

            return (address.Length > 0) ? address[0].ToString() : "";
        }

        /// <summary>
        ///     Get a string of all the host's valid MAC addresses
        ///     <returns>A string of all the host's valid MAC addresses, split by |</returns>
        /// </summary>
        public static string GetMacAddresses()
        {
            var macs = "";
            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();

                macs = adapters.Aggregate(macs, (current, adapter) => 
                    current + ("|" + string.Join(":", (from z in adapter.GetPhysicalAddress().GetAddressBytes() select z.ToString("X2")).ToArray())));

                // Remove the first |
                if (macs.Length > 0)
                    macs = macs.Substring(1);
            }
            catch (Exception ex)
            {
                LogHandler.Log(LogName, "Error getting MAC addresses");
                LogHandler.Log(LogName, "ERROR: " + ex.Message);
            }

            return macs;
        }
    }
}