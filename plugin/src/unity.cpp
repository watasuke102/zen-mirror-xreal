#include <IUnityGraphics.h>
#include <IUnityInterface.h>

#include "common.hpp"
#include "lib.hpp"
#define UNITY_EXPORT extern "C" UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API

namespace {
IUnityInterfaces *unity;
IUnityGraphics *graphics;
void
HandleDeviceEvent(UnityGfxDeviceEventType event_type)
{
  switch (event_type) {
    case kUnityGfxDeviceEventInitialize:
      lib::Init();
      break;
    case kUnityGfxDeviceEventShutdown:
      lib::Cleanup();
      break;
    default:
      LOG("unhandled: %d\n", event_type);
      break;
  }
}
}  // namespace

// Unity API
UNITY_EXPORT void
UnityPluginLoad(IUnityInterfaces *unity_interfaces)  // NOLINT
{
  unity = unity_interfaces;
  graphics = unity->Get<IUnityGraphics>();
  graphics->RegisterDeviceEventCallback(HandleDeviceEvent);
  HandleDeviceEvent(kUnityGfxDeviceEventInitialize);
}
UNITY_EXPORT void
UnityPluginUnload()
{
  graphics->UnregisterDeviceEventCallback(HandleDeviceEvent);
}
