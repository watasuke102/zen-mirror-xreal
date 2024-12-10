#pragma once

#include <zen-remote/client/remote.h>

#include <cstdint>
#include <memory>
#include <optional>
#include <unordered_map>

#include "camera.hpp"
#include "common.hpp"

namespace lib {
class Client {
 public:
  DISABLE_MOVE_AND_COPY(Client);
  Client();
  ~Client();

  void Start();
  void UpdateScene();
  void Render(int32_t camera_id);
  void Terminate();

  int32_t RegisterCamera();
  void UnregisterCamera(int32_t camera_id);
  void SetViewProjectionMat(int32_t camera_id,         //
      float v_x0, float v_x1, float v_x2, float v_x3,  //
      float v_y0, float v_y1, float v_y2, float v_y3,  //
      float v_z0, float v_z1, float v_z2, float v_z3,  //
      float v_w0, float v_w1, float v_w2, float v_w3,  //
      float p_x0, float p_x1, float p_x2, float p_x3,  //
      float p_y0, float p_y1, float p_y2, float p_y3,  //
      float p_z0, float p_z1, float p_z2, float p_z3,  //
      float p_w0, float p_w1, float p_w2, float p_w3   //
  );

 private:
  int32_t next_id_;
  std::unordered_map<int32_t, std::shared_ptr<Camera>> camera_map_;
  std::optional<std::shared_ptr<Camera>> TryGetCamera(int32_t camera_id);

  std::unique_ptr<zen::remote::client::IRemote> remote_;
};

}  // namespace lib
