@echo off
setlocal
set "script_dir=%~dp0"
for %%I in ("%script_dir%..\..") do set "solution_dir=%%~fI"
set "config=Release"

if "%~1"=="-c" (
    if /I "%~2"=="debug" (
        set "config=Debug"
    ) else if /I not "%~2"=="release" (
        echo Usage: %~nx0 [-c debug^|release]
        exit /b 1
    )
)

echo === Building .NET projects (%config%) ===

call "%script_dir%cleanDotnet.bat" || exit /b 1

for %%P in (
    "%solution_dir%\Core"
    "%solution_dir%\Core.Tests"
    "%solution_dir%\App_wpf"
) do (
    echo.
    echo --- Building %%~nxP ---
    pushd "%%P" >nul || exit /b 1
    dotnet build -c %config% -v diag || exit /b 1
    popd
)

echo.
echo === .NET build complete (%config%) ===
endlocal