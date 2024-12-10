#include "loop.hpp"

#include <android/looper.h>

#include <cstdint>
#include <exception>

#include "common.hpp"
#include "zen-remote/loop.h"

namespace {
int
LooperCallback(int fd, int events, void *data)
{
  uint32_t mask = 0;

  if (events & ALOOPER_EVENT_INPUT) {
    mask |= zen::remote::FdSource::kReadable;
  }
  if (events & ALOOPER_EVENT_OUTPUT) {
    mask |= zen::remote::FdSource::kWritable;
  }
  if (events & ALOOPER_EVENT_HANGUP) {
    mask |= zen::remote::FdSource::kHangup;
  }
  if (events & ALOOPER_EVENT_ERROR) {
    mask |= zen::remote::FdSource::kError;
  }

  try {
    auto *source = static_cast<zen::remote::FdSource *>(data);
    source->callback(fd, mask);
  } catch (const std::exception &e) {
    LOG("Exception in callback : %s", e.what());
  }

  return 1;
}
}  // namespace

namespace lib {
void
Loop::AddFd(zen::remote::FdSource *src)
{
  int events = 0;
  if (src->mask & zen::remote::FdSource::kReadable) {
    events |= ALOOPER_EVENT_INPUT;
  }
  if (src->mask & zen::remote::FdSource::kWritable) {
    events |= ALOOPER_EVENT_OUTPUT;
  }
  if (src->mask & zen::remote::FdSource::kHangup) {
    events |= ALOOPER_EVENT_HANGUP;
  }
  if (src->mask & zen::remote::FdSource::kError) {
    events |= ALOOPER_EVENT_ERROR;
  }

  ALooper_addFd(ALooper_forThread(), src->fd, ALOOPER_POLL_CALLBACK, events,
      LooperCallback, src);
}

void
Loop::RemoveFd(zen::remote::FdSource *src)
{
  ALooper_removeFd(ALooper_forThread(), src->fd);
  src->data = nullptr;
}

void
Loop::Terminate()
{
}
}  // namespace lib
