cmake_minimum_required(VERSION 3.10)
project(zen-mirror-lib LANGUAGES CXX)

set(CMAKE_EXPORT_COMPILE_COMMANDS ON)
set(CMAKE_COLOR_DIAGNOSTICS ON) # for CMake >= 3.24

if(NOT DEFINED ${UNITY_PLUGIN_API_DIR} OR ${UNITY_PLUGIN_API_DIR} STREQUAL "")
  set(UNITY_PLUGIN_API_DIR $ENV{HOME}/Unity/Hub/Editor/2022.3.53f1/Editor/Data/PluginAPI)
endif()

# setup zen-remote
set(CMAKE_CXX_FLAGS "-DBOOST_NO_CXX98_FUNCTION_BASE")
set(ZEN_REMOTE_REQUIRED_VERSION 0.1.2)
set(ZEN_REMOTE_SERVER OFF)
set(ZEN_REMOTE_CLIENT ON)
set(ZEN_REMOTE_GRAPHICS_API GLESv3)
add_subdirectory(
  ${CMAKE_CURRENT_LIST_DIR}/3rdparty/zen-remote
  zen-remote
)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_FLAGS "-Wall -Wextra -Wpedantic -Wno-gnu-zero-variadic-macro-arguments -fno-rtti")
file(
  GLOB_RECURSE lib_sources
  ${CMAKE_CURRENT_LIST_DIR}/src/*.cpp
)
file(
  GLOB_RECURSE lib_headers
  ${CMAKE_CURRENT_LIST_DIR}/inc/*.hpp
)

add_library(${PROJECT_NAME} SHARED ${lib_sources} ${lib_headers})
target_link_libraries(${PROJECT_NAME}
  zen_remote::client
  android
  log
  GLESv3
)
target_include_directories(${PROJECT_NAME} PUBLIC
  ${CMAKE_CURRENT_LIST_DIR}/inc
  ${UNITY_PLUGIN_API_DIR}
)
