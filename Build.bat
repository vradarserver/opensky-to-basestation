@echo off

:BUILD
set SOLUTION="%~dp0opensky-to-basestation.sln"

echo Cleaning binaries
call "%~dp0Clean.bat"
echo.

echo Building debug configuration
dotnet publish -c Debug
IF ERRORLEVEL 1 GOTO :ELERROR
echo.

echo Building release configuration
dotnet publish -c Release
IF ERRORLEVEL 1 GOTO :ELERROR
echo.

goto :END

:ELERROR
echo The last command returned a non-zero error level. The build failed.
exit /b 1

:END
exit /b 0
