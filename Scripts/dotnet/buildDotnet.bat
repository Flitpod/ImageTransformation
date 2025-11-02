@echo off
:: variables, save start dir
set "start_dir=%CD%"
set "solution_dir=%~dp0..\.."
set "config=Release"

:: arg parse - config setup
if "%1"=="-c" (
    if /I "%2"=="debug" (
        set "config=Debug"
    ) else if /I NOT "%2"=="release" (
        echo Usage: %0 [-c debug^|release]
        exit /b 1
    )
)

:: Clean App_wpf, Core and Core.Tests projects
call .\cleanDotnet.bat

:: build all .NET projects
for %%P in (
    "%solution_dir%\Core"
    "%solution_dir%\Core.Tests"
    "%solution_dir%\App_wpf"
) do (
    echo Building %%~nxP %config%
    cd /d "%%P"
    dotnet build -v diag -c %config%
)

:: return to start dir after execution
cd /d "%start_dir%"