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
SetViewMat(int32_t camera_id,                //
    float x0, float x1, float x2, float x3,  //
    float y0, float y1, float y2, float y3,  //
    float z0, float z1, float z2, float z3,  //
    float w0, float w1, float w2, float w3   //
)
{
  if (!client.has_value()) {
    return;
  }
  client->get()->SetViewMat(camera_id,  //
      x0, x1, x2, x3,                   //
      y0, y1, y2, y3,                   //
      z0, z1, z2, z3,                   //
      w0, w1, w2, w3                    //
  );
}
void
SetProjectionMat(int32_t camera_id,          //
    float x0, float x1, float x2, float x3,  //
    float y0, float y1, float y2, float y3,  //
    float z0, float z1, float z2, float z3,  //
    float w0, float w1, float w2, float w3   //
)
{
  if (!client.has_value()) {
    return;
  }
  client->get()->SetProjectionMat(camera_id,  //
      x0, x1, x2, x3,                         //
      y0, y1, y2, y3,                         //
      z0, z1, z2, z3,                         //
      w0, w1, w2, w3                          //
  );
}
}
}  // namespace lib
