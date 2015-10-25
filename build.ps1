Add-Type -Assemblyname System.IO.Compression.FileSystem
function Zip
{
  param([string]$inPath, [string]$zipPath)

  [System.IO.Compression.ZipFile]::CreateFromDirectory($inPath, $zipPath)
}
Write-Host $PSScriptRoot

$nuget = "$PSScriptRoot\.nuget\NuGet.exe "
$msbuild = "msbuild.exe "
$buildMode = "/verbosity:minimal /p:configuration=Debug"
$solutionConfig = "$PSScriptRoot\FOGService.sln $buildMode"
$installerConfig = "$PSScriptRoot\UniversalInstaller\UniversalInstaller.csproj $buildMode"
$msiConfig = "$PSScriptRoot\MSI\MSI.wixproj $buildMode"
$xbuild = "xbuild.bat "
$trayConfig = "$PSScriptRoot\Tray\Tray.csproj"

Write-Host "Cleaning build directory"
If (Test-Path "$PSScriptRoot\bin"){ Remove-Item -Recurse "$PSScriptRoot\bin" }

Write-Host "Restoring Packages"
Invoke-Expression ($nuget + "restore")

Write-Host "Building Solution"
Invoke-Expression ($msbuild + $solutionConfig)

Write-Host "Rebuilding Tray with mono"
Invoke-Expression ($xbuild + $trayConfig)

Write-Host "Copying Theme to build"
Copy-Item "$PSScriptRoot\themes.xml" "$PSScriptRoot\bin\themes.xml"

Write-Host "Zipping Build for installer"
$InstallerZip = "$PSScriptRoot\UniversalInstaller\Scripts\FOGService.zip"

if (Test-Path $InstallerZip) { Remove-Item $InstallerZip }
Zip "$PSScriptRoot\bin" $InstallerZip

Write-Host "Building MSI"
Invoke-Expression ($msbuild + $msiConfig)
$InstallerMSI = "$PSScriptRoot\UniversalInstaller\Scripts\FOGService.msi"
if (Test-Path $InstallerMSI) { Remove-Item $InstallerMSI }
Copy-Item "$PSScriptRoot\bin\FOGService.msi" $InstallerMSI

Write-Host "Building Installer"
Invoke-Expression ($msbuild + $installerConfig)

