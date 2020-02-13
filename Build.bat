@echo off

set                        MSBUILD="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe"
if not exist %MSBUILD% set MSBUILD="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe"
if exist %MSBUILD% goto :BUILD

echo Cannot find msbuild.exe
exit /b 1

:BUILD
set SOLUTION="%~dp0opensky-to-basestation.sln"

echo Cleaning binaries
call "%~dp0Clean.bat"
echo.

echo Restoring NuGet packages
%MSBUILD% -t:restore %SOLUTION%
IF ERRORLEVEL 1 GOTO :ELERROR
echo.

echo Building debug configuration
%MSBUILD% -t:build -property:Configuration=Debug %SOLUTION%
IF ERRORLEVEL 1 GOTO :ELERROR
echo.

echo Building release configuration
%MSBUILD% -t:build -property:Configuration=Release %SOLUTION%
IF ERRORLEVEL 1 GOTO :ELERROR
echo.

goto :END

:ELERROR
echo The last command returned a non-zero error level. The build failed.
exit /b 1

:END
exit /b 0
