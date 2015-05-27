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
using System.Linq;
using System.Net;

// ReSharper disable InconsistentNaming

namespace FOG.Handlers.Middleware
{
    public static class Communication
    {
        private const string LogName = "Middleware::Communication";

        /// <summary>
        ///     Get the parsed response of a server url
        ///     <param name="postfix">The postfix to attach to the server address</param>
        ///     <returns>The parsed response</returns>
        /// </summary>
        public static Response GetResponse(string postfix)
        {
            //ID the service as the new one
            postfix += ((postfix.Contains(".php?") ? "&" : "?") + "newService=1");

            Log.Entry(LogName, string.Format("URL: {0}{1}", Configuration.ServerAddress, postfix));

            using (var webClient = new WebClient())
            {
                try
                {
                    var rawResponse = webClient.DownloadString(Configuration.ServerAddress + postfix);
                    rawResponse = Authentication.Decrypt(rawResponse);

                    //See if the return code is known
                    var messageFound = false;
                    foreach (var returnMessage in Response.Codes.Keys.Where(returnMessage => rawResponse.StartsWith(returnMessage)))
                    {
                        messageFound = true;
                        Log.Entry(LogName, string.Format("Response: {0}", Response.Codes[returnMessage]));
                        break;
                    }

                    if (!messageFound)
                        Log.Entry(LogName, string.Format("Unknown Response: {0}", rawResponse.Replace("\n", "")));


                    if (!rawResponse.StartsWith("#!ihc")) return new Response(rawResponse);

                    return Authentication.HandShake() ? GetResponse(postfix) : new Response();
                }
                catch (Exception ex)
                {
                    Log.Error(LogName, "Could not contact FOG server");
                    Log.Error(LogName, ex);
                }                
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
                postfix += ((postfix.Contains(".php?") ? "&" : "?") + "mac=" + Configuration.MACAddresses());
           
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

            Log.Entry(LogName, "URL: " + Configuration.ServerAddress + postfix);

            using (var webClient = new WebClient())
            {
                try
                {
                    var response = webClient.DownloadString(Configuration.ServerAddress + postfix);
                    return response;
                }
                catch (Exception ex)
                {
                    Log.Error(LogName, "Could not contact FOG server");
                    Log.Error(LogName, ex);
                }               
            }

            return "";
        }

        public static Response Post(string postfix, string param)
        {
            Log.Entry(LogName, "POST URL: " + Configuration.ServerAddress + postfix);
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                    var rawResponse = webClient.UploadString(Configuration.ServerAddress + postfix, param);
                    Log.Debug(LogName, rawResponse);

                    rawResponse = Authentication.Decrypt(rawResponse);

                    var messageFound = false;
                    foreach (var returnMessage in Response.Codes.Keys.Where(returnMessage => rawResponse.StartsWith(returnMessage)))
                    {
                        messageFound = true;
                        Log.Entry(LogName, string.Format("Response: {0}", Response.Codes[returnMessage]));
                        break;
                    }

                    if (!messageFound)
                        Log.Entry(LogName, string.Format("Unknown Response: {0}", rawResponse.Replace("\n", "")));

                    return new Response(rawResponse);
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Failed to POST data");
                Log.Error(LogName, ex);
            }

            return null;
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

            Log.Entry(LogName, string.Format("URL: {0}{1}", Configuration.ServerAddress, postfix));
           
            using (var webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadString(Configuration.ServerAddress + postfix);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(LogName, "Could not contact FOG server");
                    Log.Error(LogName, ex);
                }               
            }

            return false;
        }

        public static bool Contact(string postfix, bool appendMAC)
        {
            if (appendMAC)
                postfix += ((postfix.Contains(".php?") ? "&" : "?") + "mac=" + Configuration.MACAddresses());

            return Contact(postfix);
        }

        /// <summary>
        ///     Downloads a file and creates necessary directories
        ///     <param name="postfix">The postfix to attach to the server address</param>
        ///     <param name="filePath">The location to save the file</param>
        ///     <returns>True if the download was successful</returns>
        /// </summary>
        public static bool DownloadFile(string postfix, string filePath)
        {
            return DownloadExternalFile(Configuration.ServerAddress + postfix, filePath);
        }

        public static bool DownloadExternalFile(string url, string filePath)
        {
            Log.Entry(LogName, string.Format("URL: {0}", url));
            
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filePath))
            {
                Log.Error(LogName, "Invalid parameters");
                return false;
            }

            using (var webClient = new WebClient())
            {
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
                    Log.Error(LogName, "Could not download file");
                    Log.Error(LogName, ex);
                }             
            }

            return false;
        }
    }
}