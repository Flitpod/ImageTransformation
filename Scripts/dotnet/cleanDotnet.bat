@echo off
set "solution_dir=%~dp0..\.."

:: build all .NET projects
for %%P in (
    "%solution_dir%\Core\bin"
    "%solution_dir%\Core\obj"
    "%solution_dir%\Core.Tests\bin"
    "%solution_dir%\Core.Tests\obj"
    "%solution_dir%\App_wpf\bin"
    "%solution_dir%\App_wpf\obj"
) do (
    if exist "%%P/" rmdir /s /q "%%P"
)

echo Successful App_wpf, Core, Core.Tests clean.