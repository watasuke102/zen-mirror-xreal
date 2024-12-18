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
  void SetViewMat(int32_t camera_id,           //
      float x0, float x1, float x2, float x3,  //
      float y0, float y1, float y2, float y3,  //
      float z0, float z1, float z2, float z3,  //
      float w0, float w1, float w2, float w3   //
  );
  void SetProjectionMat(int32_t camera_id,     //
      float x0, float x1, float x2, float x3,  //
      float y0, float y1, float y2, float y3,  //
      float z0, float z1, float z2, float z3,  //
      float w0, float w1, float w2, float w3   //
  );

 private:
  int32_t next_id_;
  std::unordered_map<int32_t, std::shared_ptr<Camera>> camera_map_;
  std::optional<std::shared_ptr<Camera>> TryGetCamera(int32_t camera_id);

  std::unique_ptr<zen::remote::client::IRemote> remote_;
};

}  // namespace lib
