# FOG Client

## Concept
FOG Client is a cross platform computer management software. With it, Linux, Mac, and Windows machines can easily be managed by a remote server.

## Development

Windows      | Linux       | OSX
-------------|-------------|-------------
[![Windows](https://ci.appveyor.com/api/projects/status/6uqyhjiarj0dysa8/branch/master?svg=true)](https://ci.appveyor.com/project/jbob182/fog-client/branch/master) | [![Linux](https://travis-ci.org/FOGProject/fog-client.svg?branch=master)](https://travis-ci.org/FOGProject/fog-client/) | [![OSX](https://travis-ci.org/FOGProject/fog-client.svg?branch=osx-build)](https://travis-ci.org/FOGProject/fog-client/)


#### Cross Platform Compatibility

| Feature | Windows | Linux | OSX |
|:----------------:|:-------:|:-----:|:---:|
| Auto Logout | ✓ | ✓ | ✓ |
| Auto Updating | ✓ | ✓ | ✓ |
| PowerManagement | ✓ | ✓ | ✓ |
| Rename | ✓ | ✓ | ✓ |
| Join Active Directory | ✓ |  | ✓ |
| Join Samba Directory | ✓ |  | ✓ |
| Join Open Directory | ✓ |  | ✓ |
| Snapins | ✓ | ✓ | ✓ |
| Snapin Packs | ✓ | ✓ | ✓ |
| Task Reboot | ✓ | ✓ | ✓ |
| User Tracker | ✓ | ✓ | ✓ |
| TCP/IP Printers | ✓ |  |  |
| Network Printers | ✓ |  |  |
| CUPS Printers |  | ✓ | ✓ |

## Building

#### Environment

To checkout and build the entire client (including the Installer) Windows is required. This is due to the MSI for network deployment and the Universal Installer. The following dependencies must be installed and included in PATH
* [Git for Windows](https://git-for-windows.github.io/)
  * Default install settings are fine.
* [WiX Toolset](http://wixtoolset.org/)
  * Download installer wix???.exe from github repo linked on that website.
* **(Windows 7 only/Windows 10 included already)** [Powershell / WMF 4.0+](https://www.microsoft.com/en-us/download/details.aspx?id=40855)
  * See Install Instructions on that page to find the correct package for your system
* [.NET v4.6](https://www.microsoft.com/en-us/download/details.aspx?id=48130)
* [MSBuild Tools 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48159) or [MS Visual Studio](https://visualstudio.microsoft.com/de/downloads/)
  * Included with Visual Studios 2015 if you have that already
  * Add to PATH - for instructions see below
* [Windows SDK 7, .NET 4](https://www.microsoft.com/en-us/download/details.aspx?id=8279)
  * Before running the installer **note down** and then adjust some registry keys or the installer will fail (details see [here](https://stackoverflow.com/questions/31455926/windows-sdk-setup-failure))
    * 32 bit: `HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client\Version` and `HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\Version`
    * 64 bit: `HKLM\SOFTWARE\Wow6432Node\Microsoft\NET Framework Setup\NDP\v4\Client\Version` and `HKLM\SOFTWARE\Wow6432Node\Microsoft\NET Framework Setup\NDP\v4\Full\Version`
  * Default setup is ok if you have enough disk space left but you can savely go with only selecting '.NET Development' and the two submodules
  * After installation change back the registry keys to what they were before

In case you want to build the Universal Installer you need to add ILMerge as well:
* [ILMerge](https://www.microsoft.com/en-us/download/confirmation.aspx?id=17630)
  * (Add to PATH - for instructions see below)

Adding PATH variables:
* Win+R key -> `rundll32.exe sysdm.cpl,EditEnvironmentVariables`
* New... -> Variable name: `PATH`, Variable value: `C:\Program Files\MSBuild\14.0\Bin;C:\Program Files\Microsoft\ILMerge`

**Now, restart your system!**

#### Repository and powershell setting
Open Git Bash and clone the repository:
```
IEUser@WIN7 MINGW32 ~/Desktop
$ git clone https://github.com/FOGProject/fog-client
Cloning into 'fog-client'...
...
```

Powershell must be configured to allow scripts to be run on the machine. Open CMD as adminstrator and run
```
powershell "Set-ExecutionPolicy RemoteSigned"
```

#### Build Command
```
powershell "C:\path\to\fog-client\build.ps1"
```

The binaries will be in `C:\path\to\fog-client\bin`

## Modules
The client's functionality derives from modules. Each module has 1 specific goal, and is isolated from every other module. Each module is executed in a sandbox-like environment, preventing bad code from crashing the service. Since each module is isolated, the client's server can choose which modules to enable or disable.

#### AutoLogOut
AutoLogOut is responsible for automatically logging out users after a set inactivity period. Once that time period is reached, the user is notified that if they remain inactive they will be logged out.
* On Linux AutoLogOut is only possible if `xprintidle` is installed.
* On Windows one user must be logged in for other users to log out. This is a security measure put in place by Windows. For example, if 5 users are 'logged in', but all hit the 'Switch User' button, non will be logged out. But once someone logs into the machine they will be. GPO is recommened to handle auto log outs on Windows because of this restriction.

#### PowerManagement
PowerManagement is a cron-style power management module. A computer can be configured to restart / shutdown at specific times / days. On-demand shutdown / restarts can also be issued with this module.

#### HostnameChanger
HostnameChanger is one of the core, and most used, modules of the client. It will:
* Rename a computer
* Join a domain
* Leave a domain
* Activate a windows installation with a product key

This module also handles renaming in a domain-friendly fashion. If a computer is joined to a domain, it will first leave the domain, then rename the host, and then join back. This ensures that there are no remenants in the domain.

#### PrinterManager
PrinterManager is aimed to replace the difficult GPO printer systems. Due to the complexity of printers in general, this module has one of the steepest learning curves. 

There are three printer management modes

1. **None** No printers will be added or removed, this is identical to disabling the module
2. **FOG Managed Printers** The module will only remove printers from a computer if FOG is set to manage it. For example, if a person has personal printer attached to their computer, this module will not touch it.
3. **Only Assigned Printers** This mode should be used with caution. If selected, this module will only allow assigned printers to be added to a computer. For example, if a person has a personal printer, this module will prevent the user from adding it, unless a FOG administrator adds the printer to the list of managed.

#### SnapinClient
Snapins are executables push out via the FOG server. A snapin can also be set to reboot after finishing. With this module you can push out MSIs, EXEs, batch scripts, shell scripts, or any other file capable of being executed.

#### TaskReboot
TaskReboot will automatically restart a computer if the client's server has task waiting for the computer. Example tasks include image deployment, image capturing and hardware inventory.

#### UserTracker
UserTracker will automatically report to the FOG server any logins or logouts that occur on the computer.

