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

set "test_dir=%solution_dir%\Core.Tests"

if not exist "%test_dir%\" (
    echo.
    echo *** ERROR: Test project not found ***
    echo     %test_dir%
    echo.
    exit /b 1
)

echo.
echo === Running .NET tests (%config%) ===
pushd "%test_dir%" >nul
dotnet test -c %config% -v diag
set "rc=%ERRORLEVEL%"
popd

if %rc%==0 (
    echo === All .NET tests PASSED ===
) else (
    echo === Some .NET tests FAILED (code %rc%) ===
)
exit /b %rc%