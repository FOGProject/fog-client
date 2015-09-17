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
using System.Text;

// ReSharper disable InconsistentNaming

namespace FOG.Handlers.Middleware
{
    public static class Communication
    {
        private const string LogName = "Middleware::Communication";

        /// <summary>
        ///     Get the parsed response of a server url
        /// </summary>
        /// <param name="postfix">The postfix to attach to the server address</param>
        /// <returns>The parsed response</returns>
        public static Response GetResponse(string postfix)
        {
            try
            {
                var rawResponse = GetRawResponse(postfix);
                Log.Debug(LogName, "Raw: " + rawResponse);
                var encrypted = rawResponse.StartsWith("#!en");
                Log.Debug(LogName, "ENCRYPTED: " + encrypted);

                if (encrypted)
                    rawResponse = Authentication.Decrypt(rawResponse);

                //See if the return code is known
                var messageFound = false;
                foreach (
                    var returnMessage in
                        Response.Codes.Keys.Where(returnMessage => rawResponse.StartsWith(returnMessage)))
                {
                    messageFound = true;
                    Log.Entry(LogName, $"Response: {Response.Codes[returnMessage]}");
                    break;
                }

                if (!messageFound)
                    Log.Entry(LogName, $"Unknown Response: {rawResponse.Replace("\n", "")}");


                if (!rawResponse.StartsWith("#!ihc")) return new Response(rawResponse, encrypted);

                return Authentication.HandShake() ? GetResponse(postfix) : new Response();
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not contact FOG server");
                Log.Error(LogName, ex);
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
        /// </summary>
        /// <param name="postfix">The postfix to attach to the server address</param>
        /// <returns>The unparsed response</returns>
        public static string GetRawResponse(string postfix)
        {
            //ID the service as the new one
            postfix += ((postfix.Contains(".php?") ? "&" : "?") + "newService=1");

            Log.Entry(LogName, "URL: " + Configuration.ServerAddress + postfix);

            var webRequest = WebRequest.Create(Configuration.ServerAddress + postfix);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                var result = reader.ReadToEnd();
                return result;
            }
        }

        /// <summary>
        ///     POST data to a URL
        /// </summary>
        /// <param name="postfix">The text to append to the URL</param>
        /// <param name="param">The params to post</param>
        /// <returns>The response of the server</returns>
        public static Response Post(string postfix, string param)
        {
            Log.Entry(LogName, "POST URL: " + Configuration.ServerAddress + postfix);

            try
            {
                // Create a request using a URL that can receive a post. 
                var request = WebRequest.Create(Configuration.ServerAddress + postfix);
                request.Method = "POST";

                // Create POST data and convert it to a byte array.
                var byteArray = Encoding.UTF8.GetBytes(param);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                // Get the request stream.
                var dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                // Get the response.
                var response = request.GetResponse();
                Log.Debug(LogName, "Post response = " + ((HttpWebResponse) response).StatusDescription);
                dataStream = response.GetResponseStream();

                // Open the stream using a StreamReader for easy access.
                var reader = new StreamReader(dataStream);
                var rawResponse = reader.ReadToEnd();

                // Clean up the streams.
                reader.Close();
                dataStream?.Close();
                response.Close();

                Log.Debug(LogName, rawResponse);

                var encrypted = rawResponse.StartsWith("#!en");

                if (encrypted)
                    rawResponse = Authentication.Decrypt(rawResponse);

                var messageFound = false;
                foreach (
                    var returnMessage in
                        Response.Codes.Keys.Where(returnMessage => rawResponse.StartsWith(returnMessage)))
                {
                    messageFound = true;
                    Log.Entry(LogName, $"Response: {Response.Codes[returnMessage]}");
                    break;
                }

                if (!messageFound)
                    Log.Entry(LogName, $"Unknown Response: {rawResponse.Replace("\n", "")}");

                return new Response(rawResponse, encrypted);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Failed to POST data");
                Log.Error(LogName, ex);
            }

            return new Response();
        }

        /// <summary>
        ///     GET a URL but don't check for a response
        /// </summary>
        /// <param name="postfix">The postfix to attach to the server address</param>
        /// <returns>True if the server was contacted successfully</returns>
        public static bool Contact(string postfix)
        {
            //ID the service as the new one
            postfix += ((postfix.Contains(".php?") ? "&" : "?") + "newService=1");

            Log.Entry(LogName, $"URL: {Configuration.ServerAddress}{postfix}");

            try
            {
                GetRawResponse(postfix);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not contact FOG server");
                Log.Error(LogName, ex);
            }

            return false;
        }

        /// <summary>
        ///     GET a URL but don't check for a response
        /// </summary>
        /// <param name="postfix">The text to append to the url</param>
        /// <param name="appendMAC">Should the MAC be appended</param>
        /// <returns>True if successful</returns>
        public static bool Contact(string postfix, bool appendMAC)
        {
            if (appendMAC)
                postfix += ((postfix.Contains(".php?") ? "&" : "?") + "mac=" + Configuration.MACAddresses());

            return Contact(postfix);
        }

        /// <summary>
        ///     Downloads a file and creates necessary directories
        /// </summary>
        /// <param name="postfix">The postfix to attach to the server address</param>
        /// <param name="filePath">The location to save the file</param>
        /// <returns>True if the download was successful</returns>
        public static bool DownloadFile(string postfix, string filePath)
        {
            return DownloadExternalFile(Configuration.ServerAddress + postfix, filePath);
        }

        /// <summary>
        ///     Download a file from an external server
        /// </summary>
        /// <param name="url">The URL to download from</param>
        /// <param name="filePath">The path to save the file to</param>
        /// <returns>True if successful</returns>
        public static bool DownloadExternalFile(string url, string filePath)
        {
            Log.Entry(LogName, $"URL: {url}");

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filePath))
            {
                Log.Error(LogName, "Invalid parameters");
                return false;
            }

            // Assign values to these objects here so that they can
            // be referenced in the finally block
            Stream remoteStream = null;
            Stream localStream = null;
            WebResponse response = null;

            var err = false;

            // Use a try/catch/finally block as both the WebRequest and Stream
            // classes throw exceptions upon error
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                // Create a request for the specified remote file name
                var request = WebRequest.Create(url);
                // Send the request to the server and retrieve the
                // WebResponse object 
                response = request.GetResponse();
                {
                    // Once the WebResponse object has been retrieved,
                    // get the stream object associated with the response's data
                    remoteStream = response.GetResponseStream();

                    // Create the local file
                    localStream = File.Create(filePath);

                    // Allocate a 1k buffer
                    var buffer = new byte[1024];
                    int bytesRead;

                    // Simple do/while loop to read from stream until
                    // no bytes are returned
                    do
                    {
                        // Read data (up to 1k) from the stream
                        bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                        // Write the data to the local file
                        localStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead > 0);
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not download file");
                Log.Error(LogName, ex);
                err = true;
            }
            finally
            {
                // Close the response and streams objects here 
                // to make sure they're closed even if an exception
                // is thrown at some point
                response?.Close();
                remoteStream?.Close();
                localStream?.Close();
            }

            return !err && File.Exists(filePath);
        }
    }
}