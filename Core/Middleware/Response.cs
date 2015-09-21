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
using System.Linq;
using FOG.Core.Data;

namespace FOG.Core.Middleware
{
    /// <summary>
    ///     Contains the information that the FOG Server responds with
    /// </summary>
    public class Response
    {
        private const string LogName = "Middleware::Response";
        public const string SuccessCode = "#!ok";

        public static readonly Dictionary<string, string> Codes = new Dictionary<string, string>
        {
            {SuccessCode, "Success"},
            {"#!db", "Database error"},
            {"#!im", "Invalid MAC address format"},
            {"#!ihc", "Invalid host certificate"},
            {"#!ih", "Invalid host"},
            {"#!il", "Invalid login"},
            {"#!it", "Invalid task"},
            {"#!nvp", "Invalid Printer"},
            {"#!ng", "Module is disabled globally on the FOG server"},
            {"#!nh", "Module is disabled on the host"},
            {"#!um", "Unknown module ID"},
            {"#!ns", "No snapins"},
            {"#!nj", "No jobs"},
            {"#!np", "No Printers"},
            {"#!na", "No actions"},
            {"#!nf", "No updates"},
            {"#!time", "Invalid time"},
            {"#!ist", "Invalid security token"},
            {"#!er", "General error"}
        };

        public Response(string rawData, bool encrypted)
        {
            Encrypted = encrypted;
            var data = rawData.Split('\n'); //Split the response at every new line
            var parsedData = new Dictionary<string, string>();
            try
            {
                //Get and set the error boolean
                var returnCode = data[0];
                ReturnCode = returnCode;
                Error = !returnCode.ToLower().Trim().StartsWith(SuccessCode);

                //Loop through each line returned and if it contains an '=' add it to the dictionary
                foreach (var element in data.Where(element => element.Contains("=")))
                {
                    parsedData.Add(element.Substring(0, element.IndexOf("=")).Trim(),
                        element.Substring(element.IndexOf("=") + 1).Trim());
                }

                Data = parsedData;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not parse data");
                Log.Error(LogName, ex);
            }
        }

        public Response(bool error, Dictionary<string, string> data, string returnCode, bool encrypted)
        {
            Error = error;
            Data = data;
            ReturnCode = returnCode;
            Encrypted = encrypted;
        }

        public Response()
        {
            Error = true;
            Data = new Dictionary<string, string>();
            ReturnCode = "";
            Encrypted = false;
        }

        public bool Error { get; set; }
        public bool Encrypted { get; private set; }
        public Dictionary<string, string> Data { get; set; }
        public string ReturnCode { get; set; }

        /// <summary>
        ///     Parse a Response for an array of objects
        /// </summary>
        /// <param name="identifier">The string identifier infront of the elements</param>
        /// <param name="base64Decode">Whether the elements should be base64 decoded</param>
        /// <returns>A List of the elements matching the identifier</returns>
        public List<string> GetList(string identifier, bool base64Decode)
        {
            Log.Debug(LogName, "Parsing List...");

            var items = Data.Keys.Where(key => key.Contains(identifier)).Select(key =>
                base64Decode
                    ? Transform.DecodeBase64(GetField(key))
                    : GetField(key))
                .ToList();

            foreach (var value in items)
            {
                Log.Debug(LogName, "--> " + value);
            }

            return items;
        }

        /// <summary>
        ///     Return the value stored at a specified key
        /// </summary>
        /// <param name="id">The ID to return</param>
        /// <returns>The value stored at key ID, if the ID is not present, return null</returns>
        public string GetField(string id)
        {
            return Data.ContainsKey(id) ? Data[id] : "";
        }

        /// <summary>
        ///     Check if a field is not null or empty
        /// </summary>
        /// <param name="id">The field to check</param>
        /// <returns></returns>
        public bool IsFieldValid(string id)
        {
            return !string.IsNullOrEmpty(GetField(id));
        }

        /// <summary>
        ///     Print out all ids and values
        /// </summary>
        public void PrettyPrint()
        {
            Log.Entry(LogName, "Printing values...");
            foreach (var key in Data.Keys)
                Log.Entry(LogName, "--> " + key + " = " + Data[key]);
        }
    }
}