using UnityEngine;

public class Android
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

  public enum DirectoryType
  {
    Pictures
  };
  public static string GetPublicDir(DirectoryType type_enum)
  {
    var type_str = type_enum switch
    {
      DirectoryType.Pictures => "DIRECTORY_PICTURES",
    };
    string dir;
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
      using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
      {
        using (var ctx = activity.Call<AndroidJavaObject>("getApplicationContext"))
        {
          using (var env = new AndroidJavaClass("android.os.Environment"))
          {
            using (var type = env.GetStatic<AndroidJavaObject>(type_str))
            {
              using (var path = env.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", type))
              {
                dir = path.Call<string>("toString");
              }
            }
          }
        }
      }
    }
    return dir;
  }
}
