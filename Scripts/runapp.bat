@echo off
setlocal
:: -------------------------------------------------
::  runapp.bat – Run App_wpf.exe (Release, fallback Debug)
:: -------------------------------------------------

:: 1. Script directory → solution root
set "script_dir=%~dp0"
for %%I in ("%script_dir%..") do set "solution_dir=%%~fI"

:: 2. Try Release first
set "exe=%solution_dir%\App_wpf\bin\Release\net6.0-windows\App_wpf.exe"
if exist "%exe%" (
    set "config=Release"
    goto :run
)

:: 3. Fallback to Debug
set "exe=%solution_dir%\App_wpf\bin\Debug\net6.0-windows\App_wpf.exe"
if exist "%exe%" (
    set "config=Debug"
    goto :run
)

:: 4. Not found
echo.
echo *** ERROR: App_wpf.exe not found ***
echo.
echo     Searched:
echo       %solution_dir%\App_wpf\bin\Release\net6.0-windows\App_wpf.exe
echo       %solution_dir%\App_wpf\bin\Debug\net6.0-windows\App_wpf.exe
echo.
echo Build first:
echo   Scripts\build.bat
echo.
exit /b 1

:run
echo.
echo === Launching App_wpf (%config%) ===
pushd "%~dp1" >nul 2>&1
"%exe%"
set "rc=%ERRORLEVEL%"
popd

if %rc%==0 (
    echo === App closed normally ===
) else (
    echo === App exited with code %rc% ===
)
exit /b %rc%