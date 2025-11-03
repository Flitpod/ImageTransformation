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

call "native\buildNative.bat" -c %config% || exit /b 1

call "dotnet\buildDotnet.bat" -c %config% || exit /b 1

echo === Build complete ===
endlocal