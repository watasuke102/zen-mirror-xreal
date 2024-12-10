#include "lib.hpp"

#include <cstdint>
#include <memory>
#include <optional>

#include "client.hpp"

namespace {
std::optional<std::unique_ptr<lib::Client>> client = std::nullopt;

// called via GL.IssuePluginEvent
void
Render(int32_t camera_id)
{
  if (!client.has_value()) {
    return;
  }
  client->get()->Render(camera_id);
}
// this is also intended to be called via GL.IssuePluginEvent
// because zen-remote calls OpenGL API in IRemote::UpdateScene()
void
UpdateScene(int32_t /*a*/)
{
  if (!client.has_value()) {
    return;
  }
  client->get()->UpdateScene();
}

}  // namespace

namespace lib {
extern "C" {
void
Init()
{
  if (client.has_value()) {
    return;
  }
  client = std::make_unique<Client>();
  client->get()->Start();
}

void
Cleanup()
{
  if (!client.has_value()) {
    return;
  }
  client->get()->Terminate();
  client = std::nullopt;
}

PluginEventFnPtr
GetRenderHandlerPtr()
{
  return Render;
}
PluginEventFnPtr
GetUpdateSceneFnPtr()
{
  return UpdateScene;
}

int32_t
RegisterCamera()
{
  if (!client.has_value()) {
    return -1;
  }
  return client->get()->RegisterCamera();
}

void
UnregisterCamera(int32_t camera_id)
{
  if (!client.has_value()) {
    return;
  }
  client->get()->UnregisterCamera(camera_id);
}

void
SetViewProjectionMat(int32_t camera_id,              //
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
  if (!client.has_value()) {
    return;
  }
  client->get()->SetViewProjectionMat(camera_id,  //
      v_x0, v_x1, v_x2, v_x3,                     //
      v_y0, v_y1, v_y2, v_y3,                     //
      v_z0, v_z1, v_z2, v_z3,                     //
      v_w0, v_w1, v_w2, v_w3,                     //
      p_x0, p_x1, p_x2, p_x3,                     //
      p_y0, p_y1, p_y2, p_y3,                     //
      p_z0, p_z1, p_z2, p_z3,                     //
      p_w0, p_w1, p_w2, p_w3                      //
  );
}
}
}  // namespace lib
