/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2023 FOG Project
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
using Zazzles;

namespace FOG.Modules.HostnameChanger.Linux
{
    internal class LinuxHostName : IHostName
    {
        private readonly string Name = "HostnameChanger";
        private string currentHostName;

        public bool RenameComputer(DataContracts.HostnameChanger msg)
        {
            currentHostName = Environment.MachineName;

            BruteForce(msg.Hostname);
            Power.Restart(Settings.Get("Company") + " needs to rename your computer", Power.ShutdownOptions.Delay);

            return true;
        }

        public bool RegisterComputer(DataContracts.HostnameChanger msg)
        {
            throw new NotImplementedException();
        }

        public bool UnRegisterComputer(DataContracts.HostnameChanger msg)
        {
            return true;
        }

        public void ActivateComputer(string key)
        {
            throw new NotImplementedException();
        }

        private void BruteForce(string hostname)
        {
            Log.Entry(Name, "Brute forcing hostname change...");
            UpdateHostname(hostname);
            UpdateHOSTNAME(hostname);
            UpdateHosts(hostname);
            UpdateNetwork(hostname);
        }

        private void ReplaceAll(string file, string hostname)
        {
            if (!File.Exists(file))
            {
                Log.Error(Name, "--> Did not find " + file);
                return;
            }

            try
            {
                File.WriteAllText(file, hostname);
                Log.Entry(Name, "--> Success " + file);
            }
            catch (Exception ex)
            {
                Log.Error(Name, "--> Failed " + file);
                Log.Error(Name, "----> " + ex.Message);
            }
        }

        private void UpdateHostname(string hostname)
        {
            ReplaceAll(@"/etc/hostname", hostname);
        }

        private void UpdateHOSTNAME(string hostname)
        {
            ReplaceAll(@"/etc/HOSTNAME", hostname);
        }

        private void UpdateHosts(string hostname)
        {
            var file = @"/etc/hosts";

            if (!File.Exists(file))
            {
                Log.Error(Name, "--> Did not find " + file);
                return;
            }

            try
            {
                var lines = File.ReadAllLines(file);

                for (var i = 0; i < lines.Length; i++)
                {
                    var ip = "127.0.1.1";

                    if (!lines[i].Contains(ip)) continue;

                    lines[i] = ip + "   " + hostname;
                }

                File.WriteAllLines(file, lines);
                Log.Entry(Name, "--> Success " + file);
            }
            catch (Exception ex)
            {
                Log.Error(Name, "--> Failed " + file);
                Log.Error(Name, "----> " + ex.Message);
            }
        }

        private void UpdateNetwork(string hostname)
        {
            var file = @"/etc/sysconfig/network";

            if (!File.Exists(file))
            {
                Log.Error(Name, "--> Did not find " + file);
                return;
            }

            try
            {
                var lines = File.ReadAllLines(file);

                for (var i = 0; i < lines.Length; i++)
                {
                    if (!lines[i].Trim().Contains("HOSTNAME=")) continue;
                    var parts = lines[i].Split('=');

                    if (parts.Length < 2) return;

                    if (parts[1].Contains("."))
                    {
                        var dots = parts[1].Split('.');
                        dots[0] = hostname;
                        parts[1] = string.Join(".", dots);
                    }
                    else
                    {
                        parts[1] = hostname;
                    }

                    lines[i] = parts[0] + "=" + parts[1];
                }

                File.WriteAllLines(file, lines);
                Log.Entry(Name, "--> Success " + file);
            }
            catch (Exception ex)
            {
                Log.Error(Name, "--> Failed " + file);
                Log.Error(Name, "----> " + ex.Message);
            }
        }
    }
}