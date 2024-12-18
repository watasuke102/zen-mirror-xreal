#pragma once

#include <cstdint>

using PluginEventFnPtr = void (*)(int32_t);

namespace lib {
extern "C" {
void Init();
void Cleanup();

PluginEventFnPtr GetRenderHandlerPtr();
PluginEventFnPtr GetUpdateSceneFnPtr();

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
}
}  // namespace lib
