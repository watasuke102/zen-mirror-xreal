#include "client.hpp"

#include <GLES3/gl3.h>

#include <cstdint>
#include <cstring>
#include <memory>
#include <optional>
#include <utility>

#include "common.hpp"
#include "log-sink.hpp"
#include "loop.hpp"
#include "zen-remote/client/remote.h"

namespace lib {
Client::Client() : next_id_(1)
{
  zen::remote::InitializeLogger(std::make_unique<LogSink>());
  this->remote_ = zen::remote::client::CreateRemote(std::make_unique<Loop>());
  LOG("Client is created");
}
Client::~Client() { this->remote_->DisableSession(); }

void
Client::Start()
{
  LOG("Starting gRPC server");
  this->remote_->StartGrpcServer();
  LOG("Session enabled");
  this->remote_->EnableSession();
}

void
Client::UpdateScene()
{
  this->remote_->UpdateScene();
}

void
Client::Render(int32_t camera_id)
{
  auto camera = this->TryGetCamera(camera_id);
  if (!camera.has_value()) {
    return;
  }
  glDisable(GL_CULL_FACE);
  glEnable(GL_DEPTH_TEST);
  glDepthFunc(GL_LESS);
  this->remote_->Render(camera->get()->Get());
}

void
Client::Terminate()
{
  this->remote_->DisableSession();
  LOG("Session disabled");
}

int32_t
Client::RegisterCamera()
{
  auto new_camera_id = this->next_id_;
  ++this->next_id_;
  this->camera_map_.insert(
      std::make_pair(new_camera_id, std::make_shared<Camera>()));
  LOG("New camera : %d", new_camera_id);
  return new_camera_id;
}

void
Client::UnregisterCamera(int32_t camera_id)
{
  this->camera_map_.erase(camera_id);
}

void
Client::SetViewProjectionMat(int32_t camera_id,      //
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
  auto camera_opt = this->TryGetCamera(camera_id);
  if (!camera_opt.has_value()) {
    return;
  }
  camera_opt->get()->SetViewProjectionMat(  //
      v_x0, v_x1, v_x2, v_x3,               //
      v_y0, v_y1, v_y2, v_y3,               //
      v_z0, v_z1, v_z2, v_z3,               //
      v_w0, v_w1, v_w2, v_w3,               //
      p_x0, p_x1, p_x2, p_x3,               //
      p_y0, p_y1, p_y2, p_y3,               //
      p_z0, p_z1, p_z2, p_z3,               //
      p_w0, p_w1, p_w2, p_w3                //
  );
}

std::optional<std::shared_ptr<Camera>>
Client::TryGetCamera(int32_t camera_id)
{
  try {
    auto camera = camera_map_.at(camera_id);
    return camera;
  } catch (std::out_of_range& e) {
    LOG("Camera %d not found", camera_id);
  } catch (std::exception& e) {
    LOG("Unknown exception in Client::Render() : %s", e.what());
  }
  return std::nullopt;
}

}  // namespace lib
