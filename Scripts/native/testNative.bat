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

set "exe_dir=%solution_dir%\NativeCore.Test\build\%config%\x64"
set "exe=%exe_dir%\NativeCore.Test.exe"

if not exist "%exe_dir%\" (
    echo.
    echo *** ERROR: Build folder not found ***
    echo     %exe_dir%
    echo.
    echo Build first: .\build.bat -c %config%
    echo.
    exit /b 1
)

if not exist "%exe%" (
    echo.
    echo *** ERROR: Test executable not found ***
    echo     %exe%
    echo.
    exit /b 1
)

echo.
echo === Running NativeCore.Test (%config%) ===
pushd "%exe_dir%" >nul
"%exe%" --gtest_color=yes
set "rc=%ERRORLEVEL%"
popd

if %rc%==0 (
    echo === All native tests PASSED ===
) else (
    echo === Some native tests FAILED (code %rc%) ===
)
exit /b %rc%