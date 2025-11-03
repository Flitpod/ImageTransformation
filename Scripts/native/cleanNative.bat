@echo off
setlocal
set "script_dir=%~dp0"
set "solution_dir=%script_dir%..\.."

set "target_dir=%solution_dir%\NativeCore\build"
if exist "%target_dir%\" (
    echo Removing %target_dir%
    rmdir /s /q "%target_dir%"
)

set "target_dir=%solution_dir%\NativeCore.Test\build"
if exist "%target_dir%\" (
    echo Removing %target_dir%
    rmdir /s /q "%target_dir%"
)

echo Native clean complete.
endlocal