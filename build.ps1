param(
	[switch]$sign,
	[switch]$publish,
	[string]$server  = $(if ($publish) { Read-Host "Input server address"} ),
	[string]$path    = $(if ($publish) { Read-Host "Input remote path"} ),
	[string]$user    = $(if ($publish) { Read-Host "Input user name"} ),
	[string]$channel = $(if ($publish) { Read-Host "Input channel(nightly, release-candidate, release)"} ),
	[string]$name    = $(if ($publish) { Read-Host "Input build name"} ),
	[string]$ilMerge = "ilmerge.exe",
	[string]$msbuild = "msbuild.exe",
	[string]$nuget   = "$PSScriptRoot\.nuget\NuGet.exe",
	[string]$plink   = "$PSScriptRoot\BuildTools\plink.exe",
	[string]$pscp    = "$PSScriptRoot\BuildTools\pscp.exe",
	[string]$signer  = "$PSScriptRoot\BuildTools\SignCode.cmd",
	[string]$signerSec = "$PSScriptRoot\BuildTools\SignCodeSec.cmd",
	[string]$sshKey  = "$PSScriptRoot\BuildTools\auth.ppk",
	[string]$netPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
)

##################################################
# External API
##################################################
Add-Type -Assemblyname System.IO.Compression.FileSystem

function Zip
{
  param([string]$inPath, [string]$zipPath)

  [System.IO.Compression.ZipFile]::CreateFromDirectory($inPath, $zipPath) | out-null
}

##################################################
# VARIABLE SETUP
##################################################
$nuget   = ($nuget + " ")
$ilMerge = ($ilMerge + " ")
$msbuild = ($msbuild + " ")
$plink   = ($plink + " ")
$pscp    = ($pscp + " ")

$buildMode       = "/verbosity:minimal /p:configuration=Release"
$solutionConfig  = "$PSScriptRoot\FOGService.sln $buildMode"
$installerConfig = "$PSScriptRoot\UniversalInstaller\UniversalInstaller.csproj $buildMode"
$msiConfig       = "$PSScriptRoot\MSI\MSI.wixproj $buildMode"
$trayConfig      = "$PSScriptRoot\Tray\Tray.csproj"
$toSign          = "Zazzles.dll", "Modules.dll", "FOGService.exe", "FOGShutdownGUI.exe", "FOGUpdateHelper.exe", "FOGUpdateWaiter.exe", "FOGUserService.exe", "FOGTray.exe"
$signExec        = "cmd.exe /c ""$signer"" ""$PSScriptRoot\"
$signSecExec     = "cmd.exe /c ""$signerSec"" ""$PSScriptRoot\"

$plinkConfig     = "-i ""$sshKey"" $user@$server mkdir $path" + "$channel" + "/" + "$name"
$pscpConfig      = "-i ""$sshKey"" -r $PSScriptRoot\out\* $user" + "@" + "$server" + ":" + "$path$channel" + "/" + "$name"

$printerMerge = "/ndebug /copyattrs /targetplatform:4.0``,""$netPath"" /out:""$PSScriptRoot\out\PrinterManagerHelper.exe"" ""$PSScriptRoot\bin\PrinterManagerHelper.exe"" ""$PSScriptRoot\bin\Zazzles.dll"" ""$PSScriptRoot\bin\Newtonsoft.Json.dll"" ""$PSScriptRoot\bin\Modules.dll""" 
$smartInstallerMerge = "/ndebug /copyattrs /targetplatform:4.0``,""$netPath"" /out:""$PSScriptRoot\out\SmartInstaller.exe"" ""$PSScriptRoot\bin\SmartInstaller.exe"" ""$PSScriptRoot\bin\Zazzles.dll"" ""$PSScriptRoot\bin\Newtonsoft.Json.dll"" ""$PSScriptRoot\bin\ICSharpCode.SharpZipLib.dll"" ""$PSScriptRoot\bin\SetupHelper.dll"""
$debuggerMerge = "/ndebug /copyattrs /targetplatform:4.0``,""$netPath"" /out:""$PSScriptRoot\out\Debugger.exe"" " + `
					"""$PSScriptRoot\bin\Debugger.exe"" ""$PSScriptRoot\bin\Zazzles.dll"" " + `
					"""$PSScriptRoot\bin\Newtonsoft.Json.dll"" ""$PSScriptRoot\bin\SetupHelper.dll"" " + `
					"""$PSScriptRoot\bin\Modules.dll"" ""$PSScriptRoot\bin\EngineIoClientDotNet.dll"" " + `
					"""$PSScriptRoot\bin\log4net.dll"" ""$PSScriptRoot\bin\ProcessPrivileges.dll"" " + `
					"""$PSScriptRoot\bin\SuperSocket.Common.dll"" ""$PSScriptRoot\bin\Quartz.dll"" " + `
					"""$PSScriptRoot\bin\Common.Logging.Core.dll"" ""$PSScriptRoot\bin\Common.Logging.dll"""

$toZip = "EngineIoClientDotNet.dll", "FOGService.exe", "FOGService.exe.config", "FOGShutdownGUI.exe", `
			"FOGShutdownGUI.exe.config", "FOGTray.exe", "FOGTray.exe.config", "FOGUpdateHelper.exe", `
			"FOGUpdateHelper.exe.config", "FOGUpdateWaiter.exe", "FOGUpdateWaiter.exe.config", `
			"FOGUserService.exe", "FOGUserService.exe.config", "log4net.dll", "logo.ico", `
			"MetroFramework.dll", "MetroFramework.Fonts.dll", `
			"Modules.dll", "Modules.dll.config", "Newtonsoft.Json.dll", "ProcessPrivileges.dll", `
			"SuperSocket.ClientEngine.dll", "SuperSocket.Common.dll", "SuperSocket.SocketBase.dll", `
			"SuperSocket.SocketEngine.dll", "SuperWebSocket.dll", "themes.xml", `
			"WebSocket4Net.dll", "Zazzles.dll", "Quartz.dll", "Common.Logging.dll", `
			"Common.Logging.Core.dll", "ICSharpCode.SharpZipLib.dll", "de", "fr", "nl", "no", "eu", "es", "hu", "pt", "cs", "ro", "pl", "it",

##################################################
# Initial Build
##################################################
Write-Host "Restoring Packages"
Invoke-Expression ($nuget + "restore") | out-null

Write-Host "Cleaning Projects"
Invoke-Expression ($msbuild + $solutionConfig + " /t:Clean")
Invoke-Expression ($msbuild + $installerConfig + " /t:Clean")
Invoke-Expression ($msbuild + $msiConfig + " /t:Clean")
If (Test-Path "$PSScriptRoot\bin"){ Remove-Item -Recurse "$PSScriptRoot\bin" | out-null }
If (Test-Path "$PSScriptRoot\bin"){ Remove-Item -Recurse "$PSScriptRoot\bin" | out-null }
If (Test-Path "$PSScriptRoot\bin"){ Remove-Item -Recurse "$PSScriptRoot\bin" | out-null }
New-Item -ItemType directory -Path "$PSScriptRoot\bin" | out-null

Write-Host "Building Solution"
Invoke-Expression ($msbuild + $solutionConfig) | out-null

Write-Host "Copying Theme to build"
Copy-Item "$PSScriptRoot\themes.xml" "$PSScriptRoot\bin\themes.xml"

##################################################
# Build Installers
##################################################
if ($sign) {
	$currDate = Get-Date
	$signDate = $currDate.AddDays(-550)
	if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Start-Process powershell -Verb RunAs -Wait "-NoProfile -ExecutionPolicy Bypass -Command `"Set-Date -Date '$signDate'`"";
	}
	Write-Host "Signing Binaries with time $signDate"
	foreach ($file in $toSign) {
		Write-Host "--> $file"
		Invoke-Expression($signExec + "bin\$file""") | out-null
	}
	if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Start-Process powershell -Verb RunAs -Wait "-NoProfile -ExecutionPolicy Bypass -Command `"Set-Date -Date '$currDate'`"";
	}
	Write-Host "Signing Binaries with time $currDate"
	foreach ($file in $toSign) {
		Write-Host "--> $file"
		Invoke-Expression($signSecExec + "bin\$file""") | out-null
	}
}

Write-Host "Zipping Build for installer"
$InstallerZip = "$PSScriptRoot\UniversalInstaller\Scripts\FOGService.zip"
if (Test-Path $InstallerZip) { Remove-Item $InstallerZip | out-null }
If (Test-Path "$PSScriptRoot\bin\tmp"){ Remove-Item -Recurse "$PSScriptRoot\bin\tmp" | out-null }
New-Item -ItemType directory -Path "$PSScriptRoot\bin\tmp" | out-null

foreach ($file in $toZip) {
	Copy-Item ("$PSScriptRoot\bin\" + $file) ("$PSScriptRoot\bin\tmp\" + $file) -recurse | out-null
}
Zip "$PSScriptRoot\bin\tmp" $InstallerZip | out-null

If (Test-Path "$PSScriptRoot\out"){ Remove-Item -Recurse "$PSScriptRoot\out" | out-null }
If (Test-Path "$PSScriptRoot\out"){ Remove-Item -Recurse "$PSScriptRoot\out" | out-null }
If (Test-Path "$PSScriptRoot\out"){ Remove-Item -Recurse "$PSScriptRoot\out" | out-null }
New-Item -ItemType directory -Path "$PSScriptRoot\out" | out-null

Write-Host "Building MSI"
Invoke-Expression ($msbuild + $msiConfig) | out-null
$InstallerMSI = "$PSScriptRoot\UniversalInstaller\Scripts\FOGService.msi"
if ($sign) {
	$currDate = Get-Date
	$signDate = $currDate.AddDays(-550)
	if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Start-Process powershell -Verb RunAs -Wait "-NoProfile -ExecutionPolicy Bypass -Command `"Set-Date -Date '$signDate'`"";
	}
	Write-Host "Signing MSI with time $signDate"
	Invoke-Expression($signExec + "bin\FOGService.msi""") | out-null
	if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Start-Process powershell -Verb RunAs -Wait "-NoProfile -ExecutionPolicy Bypass -Command `"Set-Date -Date '$currDate'`"";
	}
}

if (Test-Path $InstallerMSI) { Remove-Item $InstallerMSI }
Copy-Item "$PSScriptRoot\bin\FOGService.msi" $InstallerMSI
Copy-Item "$PSScriptRoot\bin\FOGService.msi" "$PSScriptRoot\out\FOGService.msi"

Write-Host "Building Smart Installer"
Invoke-Expression ($msbuild + $installerConfig) | out-null

Write-Host "ILMerging Smart Installer"
Invoke-Expression ($ilMerge + $smartInstallerMerge )

if ($sign) {
	$currDate = Get-Date
	$signDate = $currDate.AddDays(-550)
	if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Start-Process powershell -Verb RunAs -Wait "-NoProfile -ExecutionPolicy Bypass -Command `"Set-Date -Date '$signDate'`"";
	}
	Write-Host "Signing Smart Installer with time $signDate"
	Invoke-Expression($signExec + "out\SmartInstaller.exe""") | out-null
	if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Start-Process powershell -Verb RunAs -Wait "-NoProfile -ExecutionPolicy Bypass -Command `"Set-Date -Date '$currDate'`"";
	}
	Write-Host "Signing Smart Installer with time $currDate"
	Invoke-Expression($signSecExec + "out\SmartInstaller.exe""") | out-null
}

##################################################
# Build Debugger
##################################################
Write-Host "ILMerging Debugger"
Invoke-Expression ($ilMerge + $debuggerMerge)

if ($sign) {
	$currDate = Get-Date
	$signDate = $currDate.AddDays(-550)
	if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Start-Process powershell -Verb RunAs -Wait "-NoProfile -ExecutionPolicy Bypass -Command `"Set-Date -Date '$signDate'`"";
	}
	Write-Host "Signing Debugger with time $signDate"
	Invoke-Expression($signExec + "out\Debugger.exe""") | out-null
	if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Start-Process powershell -Verb RunAs -Wait "-NoProfile -ExecutionPolicy Bypass -Command `"Set-Date -Date '$currDate'`"";
	}
	Write-Host "Signing Debugger with time $currDate"
	Invoke-Expression($signSecExec + "out\Debugger.exe""") | out-null
}

##################################################
# Build PrinterManager Helper
##################################################
Write-Host "ILMerging PrinterManagerHelper"
Invoke-Expression ($ilMerge + $printerMerge)

if ($sign) {
	$currDate = Get-Date
	$signDate = $currDate.AddDays(-550)
	if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Start-Process powershell -Verb RunAs -Wait "-NoProfile -ExecutionPolicy Bypass -Command `"Set-Date -Date '$signDate'`"";
	}
	Write-Host "Signing PrinterManager Helper with time $signDate"
	Invoke-Expression($signExec + "out\PrinterManagerHelper.exe""") | out-null
	if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Start-Process powershell -Verb RunAs -Wait "-NoProfile -ExecutionPolicy Bypass -Command `"Set-Date -Date '$currDate'`"";
	}
	Write-Host "Signing PrinterManager Helper with time $currDate"
	Invoke-Expression($signSecExec + "out\PrinterManagerHelper.exe""") | out-null
}


##################################################
# Publish Build
##################################################
if ($publish) {
	Write-Host "Making remote folder"
	Invoke-Expression ($plink + $plinkConfig)
	Write-Host "Sending build to server"
	Invoke-Expression ($pscp + $pscpConfig)
}
