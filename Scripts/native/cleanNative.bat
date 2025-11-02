@echo off
set "solution_dir=%~dp0..\.."

set "target_dir=%solution_dir%\NativeCore\build"
if exist "%target_dir%\" rmdir /s /q "%target_dir%"

set "target_dir=%solution_dir%\NativeCore.Test\build"
if exist "%target_dir%\" rmdir /s /q "%target_dir%"

echo Successful Native clean.