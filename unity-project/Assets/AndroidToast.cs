using UnityEngine;

public class AndroidToast
{
  public enum ToastDuration
  {
    Short, Long,
  };
  static string ToastDurationToString(ToastDuration duration)
  {
    return duration switch
    {
      ToastDuration.Short => "LENGTH_SHORT",
      ToastDuration.Long => "LENGTH_LONG",
      _ => "",
    };
  }

  // based on https://rarafy.com/blog/2021/04/10/unity-android-toast/
  public static void ShowToast(string message, ToastDuration duration = ToastDuration.Long)
  {
    using (var unityPlayerOuter = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
      using (var activityOuter = unityPlayerOuter.GetStatic<AndroidJavaObject>("currentActivity"))
      {
        activityOuter.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
          using (var unityPlayerInner = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
          {
            using (var activityInner = unityPlayerInner.GetStatic<AndroidJavaObject>("currentActivity"))
            {
              using (var ctx = activityInner.Call<AndroidJavaObject>("getApplicationContext"))
              {
                using (var text = new AndroidJavaObject("java.lang.String", message))
                {
                  using (var toastWidget = new AndroidJavaClass("android.widget.Toast"))
                  {
                    var dur = toastWidget.GetStatic<int>(ToastDurationToString(duration));
                    using (var toast = toastWidget.CallStatic<AndroidJavaObject>("makeText", ctx, text, dur))
                    {
                      toast.Call("show");
                    }
                  }
                }
              }
            }
          }
        }));
      }
    }
  }
}
