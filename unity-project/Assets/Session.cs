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

  int app_clicked = 0;
  void Update()
  {
    if (NRInput.GetButtonDown(ControllerButton.APP))
    {
      ++app_clicked;
      if (app_clicked >= 3)
      {
        Application.Quit();
      }
    }
    else if (NRInput.IsTouching())
    {
      app_clicked = 0;
    }
    GL.IssuePluginEvent(GetUpdateSceneFnPtr(), 0);
  }

  void OnDestroy()
  {
    Cleanup();
  }
}
