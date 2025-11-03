@echo off
setlocal
set "start_dir=%CD%"

echo Cleaning native...
if exist "native" (
    pushd "native" >nul || exit /b 1
    call cleanNative.bat || exit /b 1
    popd
) else (
    echo native\ folder not found!
    exit /b 1
)

echo Cleaning dotnet...
if exist "dotnet" (
    pushd "dotnet" >nul || exit /b 1
    call cleanDotnet.bat || exit /b 1
    popd
) else (
    echo dotnet\ folder not found!
    exit /b 1
)

echo All clean.
endlocal