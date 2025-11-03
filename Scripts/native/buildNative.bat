@echo off
setlocal
set "script_dir=%~dp0"
for %%I in ("%script_dir%..\..") do set "solution_dir=%%~fI"
set "config=Release"
set "make_dir=make_release"

if "%~1"=="-c" (
    if /I "%~2"=="debug" (
        set "config=Debug"
        set "make_dir=make_debug"
    ) else if /I not "%~2"=="release" (
        echo Usage: %~nx0 [-c debug^|release]
        exit /b 1
    )
)

echo === Building NativeCore + NativeCore.Test (%config%) ===

:: Clean first
call "%script_dir%cleanNative.bat" -c %config% || exit /b 1

:: Build NativeCore
echo.
echo --- Building NativeCore ---
pushd "%solution_dir%\NativeCore" >nul || exit /b 1
cmake -G "MinGW Makefiles" -DCMAKE_BUILD_TYPE=%config% -S . -B .\build\make\%make_dir% || exit /b 1
cd /d ".\build\make\%make_dir%" || exit /b 1
mingw32-make || exit /b 1
popd

:: Build NativeCore.Test
echo.
echo --- Building NativeCore.Test ---
pushd "%solution_dir%\NativeCore.Test" >nul || exit /b 1
cmake -G "MinGW Makefiles" -DCMAKE_BUILD_TYPE=%config% -S . -B .\build\make\%make_dir% || exit /b 1
cd /d ".\build\make\%make_dir%" || exit /b 1
mingw32-make || exit /b 1
popd

echo.
echo === Native build complete (%config%) ===
endlocal