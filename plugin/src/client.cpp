#include "client.hpp"

#include <GLES3/gl3.h>

#include <cstdint>
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
Client::SetViewMat(int32_t camera_id,        //
    float x0, float x1, float x2, float x3,  //
    float y0, float y1, float y2, float y3,  //
    float z0, float z1, float z2, float z3,  //
    float w0, float w1, float w2, float w3   //
)
{
  auto camera_opt = this->TryGetCamera(camera_id);
  if (!camera_opt.has_value()) {
    return;
  }
  camera_opt->get()->SetViewMat(  //
      x0, x1, x2, x3,             //
      y0, y1, y2, y3,             //
      z0, z1, z2, z3,             //
      w0, w1, w2, w3              //
  );
}
void
Client::SetProjectionMat(int32_t camera_id,  //
    float x0, float x1, float x2, float x3,  //
    float y0, float y1, float y2, float y3,  //
    float z0, float z1, float z2, float z3,  //
    float w0, float w1, float w2, float w3   //
)
{
  auto camera_opt = this->TryGetCamera(camera_id);
  if (!camera_opt.has_value()) {
    return;
  }
  camera_opt->get()->SetProjectionMat(  //
      x0, x1, x2, x3,                   //
      y0, y1, y2, y3,                   //
      z0, z1, z2, z3,                   //
      w0, w1, w2, w3                    //
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
