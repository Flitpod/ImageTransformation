## C++ BUILD COMMANDS
g++ -D NATIVECORE_EXPORTS -O3 -ffast-math -mavx2 -mtune=native -funroll-loops -finline-functions -shared -s -o .\build\NativeCore.dll .\API\API.cpp .\API\API.h .\API\dllimpexp.h .\LinearTransformation\Matrix.cpp .\LinearTransformation\Matrix.h .\LinearTransformation\LinearTransformation.cpp .\LinearTransformation\LinearTransformation.h

cmake -G "MinGW Makefiles" -S .\NativeCore\ -B .\NativeCore\build\

### Debug
cmake -G "MinGW Makefiles" -DCMAKE_BUILD_TYPE=Debug -S . -B .\build\make\make_debug
cd .\build\make\make_debug
mingw32-make

### Release
cmake -G "MinGW Makefiles" -DCMAKE_BUILD_TYPE=Release -S . -B .\build\make\make_release
cd .\build\make\make_release
mingw32-make
