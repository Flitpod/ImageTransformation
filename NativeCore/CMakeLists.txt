cmake_minimum_required(VERSION 3.15)
project(NativeCore VERSION 1.0.0)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Enable OpenMP
find_package(OpenMP REQUIRED)

# Collect source files
file(GLOB_RECURSE SOURCES "*.cpp")
file(GLOB_RECURSE HEADERS "*.h")

# Create shared library (DLL)
add_library(${PROJECT_NAME} SHARED ${SOURCES} ${HEADERS})

# Link OpenMP
target_link_libraries(${PROJECT_NAME} PRIVATE -fopenmp)

# Export symbols for DLL
target_compile_definitions(${PROJECT_NAME} PRIVATE NATIVECORE_EXPORTS)

# GCC optimization flags (matches VS Release config)
if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    target_compile_options(${PROJECT_NAME} PRIVATE
        -O0
        -g3
        -Wall
        -Wextra
        -fopenmp
    )
    target_link_options(${PROJECT_NAME} PRIVATE
        -g
    )
    target_compile_definitions(${PROJECT_NAME} PRIVATE _DEBUG)
elseif(CMAKE_BUILD_TYPE STREQUAL "Release")
    target_compile_options(${PROJECT_NAME} PRIVATE
        -O3
        -ffast-math
        -mavx2
        -mtune=native
        -funroll-loops
        -finline-functions
        -fopenmp
    )
    target_link_options(${PROJECT_NAME} PRIVATE
        -s
        -Wl,--gc-sections
    )
endif()

# Set output directory based on configuration
set_target_properties(${PROJECT_NAME} PROPERTIES
    PREFIX ""
    OUTPUT_NAME "NativeCore"
    LIBRARY_OUTPUT_DIRECTORY_DEBUG   "${CMAKE_SOURCE_DIR}/build/Debug/x64"
    LIBRARY_OUTPUT_DIRECTORY_RELEASE "${CMAKE_SOURCE_DIR}/build/Release/x64"
    RUNTIME_OUTPUT_DIRECTORY_DEBUG   "${CMAKE_SOURCE_DIR}/build/Debug/x64"
    RUNTIME_OUTPUT_DIRECTORY_RELEASE "${CMAKE_SOURCE_DIR}/build/Release/x64"
)
