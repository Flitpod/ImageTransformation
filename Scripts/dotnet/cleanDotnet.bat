@echo off
setlocal
set "script_dir=%~dp0"
for %%I in ("%script_dir%..\..") do set "solution_dir=%%~fI"

for %%P in (
    "%solution_dir%\Core\bin"
    "%solution_dir%\Core\obj"
    "%solution_dir%\Core.Tests\bin"
    "%solution_dir%\Core.Tests\obj"
    "%solution_dir%\App_wpf\bin"
    "%solution_dir%\App_wpf\obj"
) do (
    if exist "%%P\" (
        echo Removing %%P
        rmdir /s /q "%%P"
    )
)

echo .NET clean complete.
endlocal