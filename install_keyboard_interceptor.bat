@echo off
setlocal enabledelayedexpansion

set "appPath=%~dp0"
set "appName=KeyboardInterceptor.exe"
set "appFullPath=%appPath%%appName%"

if not exist "%appFullPath%" (
    echo Error: "%appFullPath%" does not exist.
    exit /b 1
)

echo Registering "%appFullPath%" in PATH...

for /f "tokens=2*" %%a in ('reg query "HKLM\System\CurrentControlSet\Control\Session Manager\Environment" /v Path ^| find /i "path"') do set "syspath=%%b"

if defined syspath (
    setx PATH "%syspath%;%appPath%" /M
) else (
    echo Error: Could not retrieve system PATH.
    exit /b 1
)

if errorlevel 1 (
    echo Error: Failed to update system PATH.
    exit /b 1
)

echo Application registered in PATH.

endlocal

pause