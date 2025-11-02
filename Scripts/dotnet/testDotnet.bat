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

:: navigate to the Core.Tests to run NUnit unit tests
set "target_dir=%solution_dir%\Core.Tests"

if not exist "%target_dir%\" (
    echo No %config% build available!
    exit /b 1
)

:: run NUnit dotnet tests
cd /d "%target_dir%"
dotnet test -c %config% -v diag

:: return to start dir after execution
cd /d "%start_dir%"