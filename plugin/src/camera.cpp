#include "camera.hpp"

#include <mutex>

#include "zen-remote/client/camera.h"

namespace lib {
// NOLINTNEXTLINE
Camera::Camera() { std::memset(&this->camera_, 0, sizeof(this->camera_)); }
Camera::~Camera() {}

zen::remote::client::Camera*
Camera::Get()
{
  std::lock_guard<std::mutex> lock(this->mutex_);
  return &this->camera_;
}

void
Camera::SetViewMat(                          //
    float x0, float x1, float x2, float x3,  //
    float y0, float y1, float y2, float y3,  //
    float z0, float z1, float z2, float z3,  //
    float w0, float w1, float w2, float w3   //
)
{
  std::lock_guard<std::mutex> lock(this->mutex_);
  this->camera_.view.x0 = x0;
  this->camera_.view.y0 = y0;
  this->camera_.view.z0 = z0;
  this->camera_.view.w0 = w0;
  this->camera_.view.x1 = x1;
  this->camera_.view.y1 = y1;
  this->camera_.view.z1 = z1;
  this->camera_.view.w1 = w1;
  this->camera_.view.x2 = x2;
  this->camera_.view.y2 = y2;
  this->camera_.view.z2 = z2;
  this->camera_.view.w2 = w2;
  this->camera_.view.x3 = x3;
  this->camera_.view.y3 = y3;
  this->camera_.view.z3 = z3;
  this->camera_.view.w3 = w3;
}
void
Camera::SetProjectionMat(                    //
    float x0, float x1, float x2, float x3,  //
    float y0, float y1, float y2, float y3,  //
    float z0, float z1, float z2, float z3,  //
    float w0, float w1, float w2, float w3   //
)
{
  std::lock_guard<std::mutex> lock(this->mutex_);
  this->camera_.projection.x0 = x0;
  this->camera_.projection.y0 = y0;
  this->camera_.projection.z0 = z0;
  this->camera_.projection.w0 = w0;
  this->camera_.projection.x1 = x1;
  this->camera_.projection.y1 = y1;
  this->camera_.projection.z1 = z1;
  this->camera_.projection.w1 = w1;
  this->camera_.projection.x2 = x2;
  this->camera_.projection.y2 = y2;
  this->camera_.projection.z2 = z2;
  this->camera_.projection.w2 = w2;
  this->camera_.projection.x3 = x3;
  this->camera_.projection.y3 = y3;
  this->camera_.projection.z3 = z3;
  this->camera_.projection.w3 = w3;
}

}  // namespace lib
