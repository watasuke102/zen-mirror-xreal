#include "zen-remote/logger.h"

namespace lib {
class LogSink : public zen::remote::ILogSink {
  void Sink(zen::remote::Severity remote_severity, const char* pretty_function,
      const char* file, int line, const char* format, va_list vp) override;
};

}  // namespace lib
