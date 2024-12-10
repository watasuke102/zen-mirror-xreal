#pragma once

// NOLINTNEXTLINE
#include <android/log.h>
#define LOG(str, ...) \
  __android_log_print(ANDROID_LOG_DEBUG, "ZenMirror", str, ##__VA_ARGS__)

// because needless parentheses are required for the paramater `Class`
// NOLINTBEGIN
#define DISABLE_MOVE_AND_COPY(Class)        \
  Class(const Class &) = delete;            \
  Class(Class &&) = delete;                 \
  Class &operator=(const Class &) = delete; \
  Class &operator=(Class &&) = delete
// NOLINTEND
