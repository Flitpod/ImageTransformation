# Native (c++) projects

## NativeCore C++ BUILD COMMANDS with g++
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
