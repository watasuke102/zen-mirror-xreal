using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using NRKernal;
using UnityEngine;
using UnityEngine.Rendering;

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
      Android.ShowToast($"Double-tap to quit", Android.ToastDuration.Short);
    }
  }

  public static void HandleSaveImage(AsyncGPUReadbackRequest req)
  {
    if (req.hasError)
    {
      Android.ShowToast($"Failed to get image");
      return;
    }
    try
    {
      var tex = new Texture2D(req.width, req.height, TextureFormat.ARGB32, false);
      tex.LoadRawTextureData(req.GetData<Color32>());
      tex.Apply();

      var dst_dir = $"{Android.GetPublicDir(Android.DirectoryType.Pictures)}/screenshots";
      if (!Directory.Exists(dst_dir))
      {
        Directory.CreateDirectory(dst_dir);
      }
      var filename = $"ZenMirror_{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.jpg";
      File.WriteAllBytes($"{dst_dir}/{filename}", tex.EncodeToJPG());
      Android.ShowToast($"'{filename}' is saved to {dst_dir}");
    }
    catch (Exception e)
    {
      Android.ShowToast($"{e.Message}");
      Debug.LogError($"Failed to save screenshot: {e.Message}");
    }
  }

  bool isCameraBgTransparent = true;
  public void OnContrastButtonClick()
  {
    Color c = this.isCameraBgTransparent ? Color.white : Color.clear;
    foreach (var e in GameObject.FindObjectsByType<OverrideCamera>(FindObjectsSortMode.None))
    {
      e.SetBgColor(c);
    }
    this.isCameraBgTransparent = !this.isCameraBgTransparent;
  }

  [DllImport(Constant.LibName)]
  private static extern void Init();
  [DllImport(Constant.LibName)]
  private static extern void Cleanup();
  [DllImport(Constant.LibName)]
  private static extern IntPtr GetUpdateSceneFnPtr();
  void Start()
  {
    Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
    Init();
    // show status bar; from https://discussions.unity.com/t/how-to-not-cover-up-the-android-ios-top-status-bar-eg-with-time-icons-in-mobile-app/863652/8
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
      using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
      {
        activity.Call("runOnUiThread", new AndroidJavaRunnable(set_view));
      }
    }
  }
  void set_view()
  {
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
      using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
      {
        using (var window = activity.Call<AndroidJavaObject>("getWindow"))
        {
          using (var Decor = window.Call<AndroidJavaObject>("getDecorView"))
          {
            Decor.Call("requestPointerCapture");
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
