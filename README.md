# Dependencies
Project files supported on *Windows >6.0*

## Build dependencies
>.NET SDK 6 (x64)\n
>MinGW (x64) (msys2 - x64)\n
>CMake (x64)\n

# Scripts
Uses .NET SDK 6, CMake, Make, GoogleTest, MinGW. Available Windows *.bat* scripts 

## Sctipts/
```clean.bat```
- calls ```.\native\cleanNative.bat``` and ```.\dotnet\cleanDotnet.bat```

```build.bat```
- calls ```.\native\buildNative.bat``` and ```.\dotnet\buildnDotnet.bat```
- first clean and then build all projects

```test.bat```
- calls ```.\native\testNative.bat``` and ```.\dotnet\testDotnet.bat```
- runs Google test and NUnit unit tests

```runapp.bat```
- tries to find Release and Debug *App_wpf.exe* to run in the given order


## Scrips/native/
```.\cleanNative.bat``` 
- deletes .\NativeCore\build and .\NativeCore.Test\build directories


```.\buildNative.bat -c <debug/release>``` 
options 
> .\buildNative.bat -> Release
> .\buildNative.bat -c release -> Release
> .\buildNative.bat -c debug -> Debug
- runs cleanNative.bat*
- creates NativeCore/build and NativeCore.Test/build 
    - *\build\\\<Config>\x64 -> NativeCore.dll and NativeCore.Test
    - *\build\make\make_\<Config>\ -> CMake and Make configs  


```.\testNative.bat -c <debug/release>```
options 
> .\testNative.bat -> Release
> .\testNative.bat -c release -> Release
> .\testNative.bat -c debug -> Debug
- runs google test NativeCore.Test unit tests


## Scrips/dotnet/
```.\cleanDotnet.bat``` 
- deletes 
    - .\App_wpf\bin\
    - .\App_wpf\obj\
    - .\Core\bin\
    - .\Core\obj\
    - .\Core.Test\bin\
    - .\Core.Test\obj\


```.\buildDotnet.bat -c <debug/release>``` 
options 
> .\buildDotnet.bat -> Release
> .\buildDotnet.bat -c release -> Release
> .\buildDotnet.bat -c debug -> Debug
- runs cleanDotnet.bat*
- create NativeCore/build and NativeCore.Test/build 
    - *\bin\\\<Config>\net6.0-windows\
        -  **App_wpf.exe** 
        - Core.dll 
        - Core.Tests.dll


```.\testDotnet.bat -c <debug/release>```
options 
> .\testDotnet.bat -> Release
> .\testDotnet.bat -c release -> Release
> .\testDotnet.bat -c debug -> Debug
- runs Core.Tests NUnit unit tests

# Native (c++) projects

## NativeCore C++ BUILD COMMAND with g++
```
g++ -D NATIVECORE_EXPORTS -O3 -ffast-math -mavx2 -mtune=native -funroll-loops -finline-functions -shared -s -o .\build\NativeCore.dll .\API\API.cpp .\API\API.h .\API\dllimpexp.h .\LinearTransformation\Matrix.cpp .\LinearTransformation\Matrix.h .\LinearTransformation\LinearTransformation.cpp .\LinearTransformation\LinearTransformation.h
```

## CMake - Make build for NativeCore and NativeCore.Test project
### Debug
```
cmake -G "MinGW Makefiles" -DCMAKE_BUILD_TYPE=Debug -S . -B .\build\make\make_debug
cd .\build\make\make_debug
mingw32-make
```

### Release
```
cmake -G "MinGW Makefiles" -DCMAKE_BUILD_TYPE=Release -S . -B .\build\make\make_release
cd .\build\make\make_release
mingw32-make
```

## Native unit tests run
### Debug Run NativeCore Test after build
***ctest***
```
cd .\build\make\make_debug
ctest -C Debug -VV --progress
```

***google test exe***
```
cd .\build\Debug\x64
.\NativeCore.Test.exe --gtest_color=yes
```

### Release Run NativeCore Test after build
***ctest***
```
cd .\build\make\make_release
ctest -C Release -VV --progress
```

***google test exe***
```
cd .\build\Release\x64
.\NativeCore.Test.exe --gtest_color=yes
```
