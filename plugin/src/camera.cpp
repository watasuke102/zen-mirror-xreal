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
Camera::SetViewProjectionMat(                        //
    float v_x0, float v_x1, float v_x2, float v_x3,  //
    float v_y0, float v_y1, float v_y2, float v_y3,  //
    float v_z0, float v_z1, float v_z2, float v_z3,  //
    float v_w0, float v_w1, float v_w2, float v_w3,  //
    float p_x0, float p_x1, float p_x2, float p_x3,  //
    float p_y0, float p_y1, float p_y2, float p_y3,  //
    float p_z0, float p_z1, float p_z2, float p_z3,  //
    float p_w0, float p_w1, float p_w2, float p_w3   //
)
{
  std::lock_guard<std::mutex> lock(this->mutex_);
  this->camera_.view.x0 = v_x0;
  this->camera_.view.y0 = v_y0;
  this->camera_.view.z0 = v_z0;
  this->camera_.view.w0 = v_w0;
  this->camera_.view.x1 = v_x1;
  this->camera_.view.y1 = v_y1;
  this->camera_.view.z1 = v_z1;
  this->camera_.view.w1 = v_w1;
  this->camera_.view.x2 = v_x2;
  this->camera_.view.y2 = v_y2;
  this->camera_.view.z2 = v_z2;
  this->camera_.view.w2 = v_w2;
  this->camera_.view.x3 = v_x3;
  this->camera_.view.y3 = v_y3;
  this->camera_.view.z3 = v_z3;
  this->camera_.view.w3 = v_w3;
  this->camera_.projection.x0 = p_x0;
  this->camera_.projection.y0 = p_y0;
  this->camera_.projection.z0 = p_z0;
  this->camera_.projection.w0 = p_w0;
  this->camera_.projection.x1 = p_x1;
  this->camera_.projection.y1 = p_y1;
  this->camera_.projection.z1 = p_z1;
  this->camera_.projection.w1 = p_w1;
  this->camera_.projection.x2 = p_x2;
  this->camera_.projection.y2 = p_y2;
  this->camera_.projection.z2 = p_z2;
  this->camera_.projection.w2 = p_w2;
  this->camera_.projection.x3 = p_x3;
  this->camera_.projection.y3 = p_y3;
  this->camera_.projection.z3 = p_z3;
  this->camera_.projection.w3 = p_w3;
}

}  // namespace lib
