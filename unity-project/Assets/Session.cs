using System;
using System.Collections;
using System.Runtime.InteropServices;
using NRKernal;
using UnityEngine;

public class Session : MonoBehaviour
{
  [SerializeField] OverrideCamera captureTarget;
  public void OnPhotoButtonClick()
  {
    this.captureTarget.RequestCapture();
  }

  System.DateTime prevTapTime = System.DateTime.MinValue;
  public void OnExitButtonClick()
  {
    var timespan = (System.DateTime.Now - this.prevTapTime).TotalSeconds;
    if (timespan <= 1)
    {
      Application.Quit();
    }
    this.prevTapTime = System.DateTime.Now;
    StartCoroutine(toastCoroutine());
    IEnumerator toastCoroutine()
    {
      yield return new WaitForSeconds(0.5f);
      AndroidToast.ShowToast($"Double-tap to quit", AndroidToast.ToastDuration.Short);
    }
  }

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
    // show status bar; from https://discussions.unity.com/t/how-to-not-cover-up-the-android-ios-top-status-bar-eg-with-time-icons-in-mobile-app/863652/8
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
      using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
      {
        using (var window = activity.Call<AndroidJavaObject>("getWindow"))
        {
          using (var Decor = window.Call<AndroidJavaObject>("getDecorView"))
          {
            using (var controller = Decor.Call<AndroidJavaObject>("getWindowInsetsController"))
            {
              using (var type = new AndroidJavaClass("android.view.WindowInsets$Type"))
              {
                controller.Call("show", type.CallStatic<int>("statusBars"));
              }
            }
          }
          window.Call("setFlags", 512, 512);
          window.Call("setStatusBarColor", unchecked((int)0x00005700)); //for transparent status bar
        }
      }
    }
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
