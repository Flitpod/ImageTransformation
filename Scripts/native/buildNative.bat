@echo off
:: variables, save start dir
set "start_dir=%CD%"
set "solution_dir=%~dp0..\.."
set "config=Release"
set "make_dir=make_release"

:: arg parse - config setup
if "%1"=="-c" (
    if /I "%2"=="debug" (
        set "config=Debug"
        set "make_dir=make_debug"
    ) else if /I NOT "%2"=="release" (
        echo Usage: %0 [-c debug^|release]
        exit /b 1
    )
)

:: Clean NativeCore and NativeCore.Test project
call .\cleanNative.bat

:: navigate to the NativeCore project
set "target_dir=%solution_dir%\NativeCore"
cd "%target_dir%"

:: cmake - make build Native Core
cmake -G "MinGW Makefiles" -DCMAKE_BUILD_TYPE=%config% -S . -B .\build\make\%make_dir%
cd .\build\make\%make_dir%
mingw32-make

:: navigate to the NativeCore.Test project
set "target_dir=%solution_dir%\NativeCore.Test"
cd "%target_dir%"

:: cmake - make build Native Core Test
cmake -G "MinGW Makefiles" -DCMAKE_BUILD_TYPE=%config% -S . -B .\build\make\%make_dir%
cd .\build\make\%make_dir%
mingw32-make

:: return to start dir after execution
cd /d "%start_dir%"