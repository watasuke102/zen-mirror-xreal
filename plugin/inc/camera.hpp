#pragma once

#include <mutex>

#include "common.hpp"
#include "zen-remote/client/camera.h"

namespace lib {
class Camera {
 public:
  DISABLE_MOVE_AND_COPY(Camera);
  Camera();
  ~Camera();

  zen::remote::client::Camera* Get();
  void SetViewMat(                             //
      float x0, float x1, float x2, float x3,  //
      float y0, float y1, float y2, float y3,  //
      float z0, float z1, float z2, float z3,  //
      float w0, float w1, float w2, float w3   //
  );
  void SetProjectionMat(                       //
      float x0, float x1, float x2, float x3,  //
      float y0, float y1, float y2, float y3,  //
      float z0, float z1, float z2, float z3,  //
      float w0, float w1, float w2, float w3   //
  );

 private:
  std::mutex mutex_;
  zen::remote::client::Camera camera_;
};
}  // namespace lib
