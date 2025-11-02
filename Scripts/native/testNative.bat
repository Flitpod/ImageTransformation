@echo off
:: variables, save start dir
set "start_dir=%CD%"
set "solution_dir=%~dp0..\.."
set "config=Release"
set "make_dir=make_release"

:: arg parse - config setup
if "%1"=="-c" (
    if /I "%~2"=="debug" (
        set "config=Debug"
        set "make_dir=make_debug"
    ) else if /I not "%~2"=="release" (
        echo Usage: %0 [-c debug^|release]
        exit /b 1
    )
)

:: navigate to the NativeCore.Test google test exe
set "target_dir=%solution_dir%\NativeCore.Test\build\%config%\x64"

if not exist "%target_dir%\" (
    echo No %config% build available!
    exit /b 1
)

:: run google test
cd /d "%target_dir%"
.\NativeCore.Test.exe --gtest_color=yes

cd /d "%start_dir%"