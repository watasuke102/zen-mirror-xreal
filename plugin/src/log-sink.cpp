#include "log-sink.hpp"

#include <android/log.h>

#include "zen-remote/logger.h"

namespace lib {
void
LogSink::Sink(zen::remote::Severity severity, const char* /*pretty_function*/,
    const char* /*file*/, int /*line*/, const char* format, va_list vp)
{
  using zen::remote::Severity;
  android_LogPriority priority = ANDROID_LOG_UNKNOWN;
  switch (severity) {
    case Severity::DEBUG:
      priority = ANDROID_LOG_DEBUG;
      break;
    case Severity::INFO:
      priority = ANDROID_LOG_INFO;
      break;
    case Severity::WARN:
      priority = ANDROID_LOG_WARN;
      break;
    case Severity::ERROR:
      priority = ANDROID_LOG_ERROR;
      break;
    case Severity::FATAL:
      priority = ANDROID_LOG_FATAL;
      break;
    default:
      break;
  }
  __android_log_vprint(priority, "ZenRemote", format, vp);
}

}  // namespace lib
