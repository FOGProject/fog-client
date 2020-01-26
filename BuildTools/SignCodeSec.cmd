@ECHO OFF
REM sign the file...
REM https://support.globalsign.com/customer/en/portal/articles/2958314-dual-code-signing
REM Using signtool.exe from http://cdn1.ksoftware.net/signtool_8.1.zip or full SDK for Win 8.1
BuildTools\signtool\signtool.exe sign /as /n "FOG Project - Sebastian Roth" /td SHA256 /fd SHA256 %1
echo SignCodeSec.bat exit code is 0.
exit /b 0