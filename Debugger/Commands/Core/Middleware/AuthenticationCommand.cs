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
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using FOG.Core;
using FOG.Core.Data;
using FOG.Core.Middleware;

namespace FOG.Commands.Core.Middleware
{
    internal class AuthenticationCommand : ICommand
    {
        private const string LogName = "Console::Middleware::Authentication";

        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }

            if (args[0].Equals("handshake"))
            {
                Authentication.HandShake();
                return true;
            }

            if (args[0].Equals("pin"))
            {
                try
                {
                    var keyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tmp",
                        "fog.ca.crt");
                    var downloaded = Communication.DownloadFile("/management/other/ca.cert.der", keyPath);

                    if (!downloaded)
                    {
                        Log.Error(LogName, "Failed to download CA cert");
                        return true;
                    }

                    var caCert = new X509Certificate2(keyPath);
                    RSA.ServerCertificate();
                }
                catch (Exception ex)
                {
                    Log.Error(LogName, ex);
                }


                return true;
            }

            return false;
        }

        private static void Help()
        {
            Log.WriteLine("Available commands");
            Log.WriteLine("--> handshake");
            Log.WriteLine("--> pin");
        }
    }
}