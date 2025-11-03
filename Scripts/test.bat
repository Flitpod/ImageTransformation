@echo off
setlocal
set "config=Release"

if "%~1"=="-c" (
    if /I "%~2"=="debug" (
        set "config=Debug"
    ) else if /I not "%~2"=="release" (
        echo Usage: %~nx0 [-c debug^|release]
        exit /b 1
    )
)

call "native\testNative.bat" -c %config% || exit /b 1

call "dotnet\testDotnet.bat" -c %config% || exit /b 1

echo === All tests done ===
endlocal