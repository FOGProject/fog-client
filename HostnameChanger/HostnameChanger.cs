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
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

namespace FOG
{
    /// <summary>
    /// Rename a host, register with AD, and activate the windows key
    /// </summary>
    public class HostnameChanger:AbstractModule
    {
		
        //Import dll methods
        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)] 
        private static extern int NetJoinDomain(string lpServer, string lpDomain, string lpAccountOU, 
            string lpAccount, string lpPassword, JoinOptions NameType);
		
        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetUnjoinDomain(string lpServer, string lpAccount, string lpPassword, UnJoinOptions fUnjoinOptions);

        [Flags]
        private enum UnJoinOptions
        {
            NONE = 0x00000000,
            NETSETUP_ACCOUNT_DELETE = 0x00000004
        }
		
        [Flags]
        private enum JoinOptions
        {
            NETSETUP_JOIN_DOMAIN = 0x00000001,
            NETSETUP_ACCT_CREATE = 0x00000002,
            NETSETUP_ACCT_DELETE = 0x00000004,
            NETSETUP_WIN9X_UPGRADE = 0x00000010,
            NETSETUP_DOMAIN_JOIN_IF_JOINED = 0x00000020,
            NETSETUP_JOIN_UNSECURE = 0x00000040,
            NETSETUP_MACHINE_PWD_PASSED = 0x00000080,
            NETSETUP_DEFER_SPN_SET = 0x10000000
        }
		
        private Dictionary<int, String> adErrors;
        private int successIndex;
        private Boolean notifiedUser;
		
    
        public HostnameChanger()
        {
            Name = "HostnameChanger";
            Description = "Rename a host, register with AD, and activate the windows key";		
			
            setADErrors();
            this.notifiedUser = false;
        }
	 
	    
        private void setADErrors()
        {
            this.adErrors = new Dictionary<int, String>();
            this.successIndex = 0;
	      	
            this.adErrors.Add(this.successIndex, "Success");
            this.adErrors.Add(5, "Access Denied");
        }
		
        protected override void doWork()
        {
            //Get task info
            var taskResponse = CommunicationHandler.GetResponse("/service/hostname.php?moduleid=" + Name.ToLower(), true);
			
            if (!taskResponse.Error)
            {
                renameComputer(taskResponse);
                if (!ShutdownHandler.ShutdownPending)
                    registerComputer(taskResponse);
                if (!ShutdownHandler.ShutdownPending)
                    activateComputer(taskResponse);
            }
        }
		
        //Rename the computer and remove it from active directory
        private void renameComputer(Response taskResponse)
        {
            LogHandler.Log(Name, taskResponse.getField("#hostname") + ":" + System.Environment.MachineName);
            if (!taskResponse.getField("#hostname").Equals(""))
            {
                if (!Environment.MachineName.ToLower().Equals(taskResponse.getField("#hostname").ToLower()))
                {
				
                    LogHandler.Log(Name, "Renaming host to " + taskResponse.getField("#hostname"));
                    if (!UserHandler.IsUserLoggedIn() || taskResponse.getField("#force").Equals("1"))
                    {
                        LogHandler.Log(Name, "Unregistering computer");
                        //First unjoin it from active directory
                        unRegisterComputer(taskResponse);		
		
                        LogHandler.Log(Name, "Updating registry");
                        RegistryKey regKey;
			
                        regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", true);
                        regKey.SetValue("NV Hostname", taskResponse.getField("#hostname"));
                        regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName", true);
                        regKey.SetValue("ComputerName", taskResponse.getField("#hostname"));
                        regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName", true);
                        regKey.SetValue("ComputerName", taskResponse.getField("#hostname"));	
						
                        ShutdownHandler.Restart(NotificationHandler.Company + " needs to rename your computer", 10);
                    }
                    else if (!this.notifiedUser)
                    {
                        LogHandler.Log(Name, "User is currently logged in, will try again later");
                        //Notify the user they should log off if it is not forced
                        NotificationHandler.Notifications.Add(new Notification("Please log off", NotificationHandler.Company +
                                " is attemping to service your computer, please log off at the soonest available time",
                                120));
						
                        this.notifiedUser = true;
                    }
                }
                else
                {
                    LogHandler.Log(Name, "Hostname is correct");
                }
            } 
	      	
        }
		
        //Add a host to active directory
        private void registerComputer(Response taskResponse)
        {
            if (taskResponse.getField("#AD").Equals("1"))
            { 
                LogHandler.Log(Name, "Adding host to active directory");
                if (!taskResponse.getField("#ADDom").Equals("") && !taskResponse.getField("#ADUser").Equals("") &&
                    !taskResponse.getField("#ADPass").Equals(""))
                {
				
                    String userPassword = taskResponse.getField("#ADPass");

                    int returnCode = NetJoinDomain(null, taskResponse.getField("#ADDom"), taskResponse.getField("#ADOU"), 
                                         taskResponse.getField("#ADUser"), userPassword, 
                                         (JoinOptions.NETSETUP_JOIN_DOMAIN | JoinOptions.NETSETUP_ACCT_CREATE));
                    if (returnCode == 2224)
                    {
                        returnCode = NetJoinDomain(null, taskResponse.getField("#ADDom"), taskResponse.getField("#ADOU"), 
                            taskResponse.getField("#ADUser"), userPassword, JoinOptions.NETSETUP_JOIN_DOMAIN);				
                    }
					
                    //Log the response
                    if (this.adErrors.ContainsKey(returnCode))
                    {
                        LogHandler.Log(Name, this.adErrors[returnCode] + " Return code: " + returnCode.ToString());
                    }
                    else
                    {
                        LogHandler.Log(Name, "Unknown return code: " + returnCode.ToString());
                    }	
					
                    if (returnCode.Equals(this.successIndex))
                        ShutdownHandler.Restart("Host joined to active directory, restart needed", 20);
					
                }
                else
                {
                    LogHandler.Log(Name, "Unable to remove host from active directory");
                    LogHandler.Log(Name, "ERROR: Not all active directory fields are set");
                }
            }
            else
            {
                LogHandler.Log(Name, "Active directory is disabled");
            }
        }
		
        //Remove the host from active directory
        private void unRegisterComputer(Response taskResponse)
        {
            LogHandler.Log(Name, "Removing host from active directory");
            if (!taskResponse.getField("#ADUser").Equals("") && !taskResponse.getField("#ADPass").Equals(""))
            {
				
                String userPassword = taskResponse.getField("#ADPass");
                int returnCode = NetUnjoinDomain(null, taskResponse.getField("#ADUser"), userPassword, UnJoinOptions.NETSETUP_ACCOUNT_DELETE);
				
                //Log the response
                if (this.adErrors.ContainsKey(returnCode))
                {
                    LogHandler.Log(Name, this.adErrors[returnCode] + " Return code: " + returnCode.ToString());
                }
                else
                {
                    LogHandler.Log(Name, "Unknown return code: " + returnCode);
                }
				
                if (returnCode.Equals(this.successIndex))
                    ShutdownHandler.Restart("Host joined to active directory, restart needed", 20);
            }
            else
            {
                LogHandler.Log(Name, "Unable to remove host from active directory, some settings are empty");
            }
        }
		
        //Active a computer with a product key
        private void activateComputer(Response taskResponse)
        {
            if (taskResponse.Data.ContainsKey("#Key"))
            {
                LogHandler.Log(Name, "Activing host with product key");
				
                //The standard windows key is 29 characters long -- 5 sections of 5 characters with 4 dashes (5*5+4)
                if (taskResponse.getField("#Key").Length == 29)
                {
                    var process = new Process();
					
                    //Give windows the new key
                    process.StartInfo.FileName = @"cscript";
                    process.StartInfo.Arguments = "//B //Nologo " + Environment.SystemDirectory + @"\slmgr.vbs /ipk " + taskResponse.getField("#Key");
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.Start();
                    process.WaitForExit();
                    process.Close();
					
                    //Try and activate the new key
                    process.StartInfo.Arguments = "//B //Nologo " + Environment.SystemDirectory + @"\slmgr.vbs /ato";
                    process.Start();
                    process.WaitForExit();
                    process.Close();
                }
                else
                {
                    LogHandler.Log(Name, "Unable to activate windows");
                    LogHandler.Log(Name, "ERROR: Invalid product key");
                }
            }
            else
            {
                LogHandler.Log(Name, "Windows activation disabled");				
            }
        }
		
    }
}