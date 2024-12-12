using System;
using System.Runtime.InteropServices;
using NRKernal;
using UnityEngine;

public class Session : MonoBehaviour
{
  [DllImport(Constant.LibName)]
  private static extern void Init();
  [DllImport(Constant.LibName)]
  private static extern void Cleanup();
  [DllImport(Constant.LibName)]
  private static extern IntPtr GetUpdateSceneFnPtr();

  void Start()
  {
    NRDebugger.logLevel = LogLevel.Warning;
    Init();
  }

  void Update()
  {
    GL.IssuePluginEvent(GetUpdateSceneFnPtr(), 0);
  }

  void OnDestroy()
  {
    Cleanup();
  }
}
