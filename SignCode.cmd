@ECHO OFF
REM create an array of timestamp servers...
REM IMPORTANT NOTE - The SET statement and the four servers should be all on one line.
set SERVERLIST=(http://timestamp.comodoca.com/authenticode http://timestamp.verisign.com/scripts/timstamp.dll http://timestamp.globalsign.com/scripts/timestamp.dll http://tsa.starfieldtech.com)
REM sign the file...
signtool.exe sign /n "Open Source Developer, Joseph Schmitt" %1
set timestampErrors=0
for /L %%a in (1,1,300) do (
    for %%s in %SERVERLIST% do (
        Echo Try %%s
        REM try to timestamp the file. This operation is unreliable and may need to be repeated...
        signtool.exe timestamp /t %%s %1
        REM check the return value of the timestamping operation and retry
        if ERRORLEVEL 0 if not ERRORLEVEL 1 GOTO succeeded
        echo Signing problem - timestamp server %%s
        set /a timestampErrors+=1
        Rem Wait 6 seconds
        choice /N /T:6 /D:Y >NUL
    )
    REM wait 12 seconds...
    choice /N /T:12 /D:Y >NUL
)
REM return an error code...
echo SignCode.bat exit code is 1. %timestampErrors% timestamping errors.
exit /b 1
:succeeded
REM return a successful code...
echo SignCode.bat exit code is 0. %timestampErrors% timestamping errors.
exit /b 0