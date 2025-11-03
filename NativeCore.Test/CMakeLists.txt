cmake_minimum_required(VERSION 3.15)
project(NativeCore.Test VERSION 1.0.0)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_DISABLE_IN_SOURCE_BUILD ON)

# Fetch GoogleTest
include(FetchContent)
FetchContent_Declare(
    googletest
    URL https://github.com/google/googletest/archive/refs/tags/v1.17.0.zip
    DOWNLOAD_EXTRACT_TIMESTAMP TRUE
)
set(BUILD_SHARED_LIBS ON)
FetchContent_MakeAvailable(googletest)

# Collect test sources
file(GLOB_RECURSE TEST_SOURCES "*.cpp")
file(GLOB_RECURSE TEST_HEADERS "*.h")
list(FILTER TEST_SOURCES EXCLUDE REGEX ".*CMakeFiles.*")

# Create test executable
add_executable(${PROJECT_NAME} ${TEST_SOURCES} ${TEST_HEADERS})

# Link GoogleTest
target_link_libraries(${PROJECT_NAME} PRIVATE GTest::gtest_main)

# NativeCore build dir
set(NATIVECORE_BUILD_DIR "${CMAKE_SOURCE_DIR}/../NativeCore/build")

# Link import lib and set output dir per config
if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    target_link_libraries(${PROJECT_NAME} PRIVATE
        "${NATIVECORE_BUILD_DIR}/make/make_debug/libNativeCore.dll.a"
    )
    set_target_properties(${PROJECT_NAME} PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/build/Debug/x64"
    )
elseif(CMAKE_BUILD_TYPE STREQUAL "Release")
    target_link_libraries(${PROJECT_NAME} PRIVATE
        "${NATIVECORE_BUILD_DIR}/make/make_release/libNativeCore.dll.a"
    )
    set_target_properties(${PROJECT_NAME} PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/build/Release/x64"
    )
endif()

# Copy NativeCore.dll
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        "${NATIVECORE_BUILD_DIR}/${CMAKE_BUILD_TYPE}/x64/NativeCore.dll"
        "$<TARGET_FILE_DIR:${PROJECT_NAME}>/"
    COMMENT "Copying NativeCore.dll"
)

# Copy GoogleTest DLLs
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        "${CMAKE_BINARY_DIR}/bin/libgtest.dll"
        "$<TARGET_FILE_DIR:${PROJECT_NAME}>/"
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        "${CMAKE_BINARY_DIR}/bin/libgtest_main.dll"
        "$<TARGET_FILE_DIR:${PROJECT_NAME}>/"
    COMMENT "Copying GoogleTest DLLs"
)

# Copy MinGW runtime DLLs
if(MINGW)
    get_filename_component(MINGW_BIN_DIR ${CMAKE_CXX_COMPILER} DIRECTORY)
    foreach(dll IN ITEMS libgomp-1.dll libgcc_s_seh-1.dll libstdc++-6.dll libwinpthread-1.dll)
        add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
            COMMAND ${CMAKE_COMMAND} -E copy_if_different
                "${MINGW_BIN_DIR}/${dll}"
                "$<TARGET_FILE_DIR:${PROJECT_NAME}>/"
            COMMENT "Copying ${dll}"
        )
    endforeach()
endif()

# Match NativeCore optimization flags
if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    target_compile_options(${PROJECT_NAME} PRIVATE -O0 -g3 -Wall -Wextra -fopenmp)
elseif(CMAKE_BUILD_TYPE STREQUAL "Release")
    target_compile_options(${PROJECT_NAME} PRIVATE
        -O3 -ffast-math -mavx2 -mtune=native -funroll-loops -finline-functions -fopenmp
    )
endif()

# Enable testing and discover GoogleTest tests
enable_testing()
include(GoogleTest)
gtest_discover_tests(${PROJECT_NAME}
    WORKING_DIRECTORY $<TARGET_FILE_DIR:${PROJECT_NAME}>
    DISCOVERY_MODE PRE_TEST
)