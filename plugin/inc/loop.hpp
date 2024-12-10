#pragma once

#include <zen-remote/loop.h>

namespace lib {

class Loop : public zen::remote::ILoop {
 public:
  void AddFd(zen::remote::FdSource *src) override;
  void RemoveFd(zen::remote::FdSource *src) override;
  void Terminate() override;
};

}  // namespace lib
