using System;
using System.IO;
using System.Runtime.InteropServices;
using NRKernal;
using UnityEngine;

public enum EyeType
{
  Left, Center, Right,
}

public class OverrideCamera : MonoBehaviour
{
  [SerializeField] EyeType eye_type;
  Camera cam;
  Int32 camera_id = 0;

  [DllImport(Constant.LibName)]
  private static extern IntPtr GetRenderHandlerPtr();
  [DllImport(Constant.LibName)]
  private static extern Int32 RegisterCamera(Int32 width, Int32 height);
  [DllImport(Constant.LibName)]
  private static extern void UnregisterCamera(Int32 camera_id);
  [DllImport(Constant.LibName)]
  private static extern void SetViewMat(Int32 camera_id,//
      float x0, float x1, float x2, float x3,  //
      float y0, float y1, float y2, float y3,  //
      float z0, float z1, float z2, float z3,  //
      float w0, float w1, float w2, float w3   //
  );
  [DllImport(Constant.LibName)]
  private static extern void SetProjectionMat(Int32 camera_id,//
      float x0, float x1, float x2, float x3,  //
      float y0, float y1, float y2, float y3,  //
      float z0, float z1, float z2, float z3,  //
      float w0, float w1, float w2, float w3   //
  );

  Matrix4x4 GetProjMat()
  {
    NativeDevice eye;
    switch (this.eye_type)
    {
      case EyeType.Left: eye = NativeDevice.LEFT_DISPLAY; break;
      case EyeType.Center: eye = NativeDevice.HEAD_CENTER; break;
      case EyeType.Right: eye = NativeDevice.RIGHT_DISPLAY; break;
      default:
        Debug.LogError($"Invalid eye_type of `{this.name}` : {this.eye_type}");
        return Matrix4x4.zero;
    }
    var fov = new NativeFov4f();
    NRFrame.GetEyeFov(eye, ref fov);

    const float near = 0.1f, far = 100.0f;
    float width = fov.right_tan - fov.left_tan;
    float height = fov.top_tan - fov.bottom_tan;
    var proj_mat = Matrix4x4.zero;
    proj_mat[0, 0] = 2.0f / width;
    proj_mat[0, 2] = (fov.right_tan + fov.left_tan) / width;
    proj_mat[1, 1] = 2.0f / height;
    proj_mat[1, 2] = (fov.top_tan + fov.bottom_tan) / height;
    proj_mat[2, 2] = far / (near - far);
    proj_mat[2, 3] = (far * near) / (near - far);
    proj_mat[3, 2] = -1;
    // var proj_mat = Matrix4x4.Perspective((float)Math.PI / 4.0f, this.cam.pixelWidth / this.cam.pixelHeight, 0.1f, 100.0f);
    return proj_mat;
  }

  void Start()
  {
    this.cam = GetComponent<Camera>();
    var width = this.cam.pixelWidth;
    var height = this.cam.pixelHeight;
    this.camera_id = RegisterCamera(width, height);
    var proj_mat = GetProjMat();
    SetProjectionMat(this.camera_id, //
      proj_mat[0, 0], proj_mat[0, 1], proj_mat[0, 2], proj_mat[0, 3], //
      proj_mat[1, 0], proj_mat[1, 1], proj_mat[1, 2], proj_mat[1, 3], //
      proj_mat[2, 0], proj_mat[2, 1], proj_mat[2, 2], proj_mat[2, 3], //
      proj_mat[3, 0], proj_mat[3, 1], proj_mat[3, 2], proj_mat[3, 3]  //
    );
    Debug.Log($"[New Camera] {this.cam.name}: pixel={width}x{height}");
  }
  void Update()
  {
    this.cam.transform.position = new Vector3(0.0f, 0.85f, 0.0f);
    var q = this.cam.transform.rotation;
    // FIXME: Rotation should be modified for proper view matrix, but why?
    //        According to the document, Camera.worldToCameraMatrix is following OpenGL convention...
    this.cam.transform.rotation = new Quaternion(-q.x, -q.y, q.z, q.w);

    bool is_succeeded;
    var proj = NRFrame.GetEyeProjectMatrix(out is_succeeded, 0.01f, 100.0f);
    Matrix4x4 proj_mat;
    if (is_succeeded)
    {
      switch (this.eye_type)
      {
        case EyeType.Left: proj_mat = proj.LEyeMatrix; break;
        case EyeType.Center: proj_mat = proj.CEyeMatrix; break;
        case EyeType.Right: proj_mat = proj.REyeMatrix; break;
        default:
          Debug.LogError($"Invalid eye_type of `{this.name}` : {this.eye_type}");
          return;
      }
    }
    else
    {
      proj_mat = Matrix4x4.zero;
      // from real XREAL Air 2 Pro; NRFrame.GetEyeProjectMatrix(_, 0.01f, 100.0f);
      proj_mat[0, 0] = +2.80947f; proj_mat[0, 1] = +0.00000f; proj_mat[0, 2] = +0.03892f; proj_mat[0, 3] = +0.00000f;
      proj_mat[1, 0] = +0.00000f; proj_mat[1, 1] = +5.02696f; proj_mat[1, 2] = +0.04567f; proj_mat[1, 3] = +0.00000f;
      proj_mat[2, 0] = +0.00000f; proj_mat[2, 1] = +0.00000f; proj_mat[2, 2] = -1.00020f; proj_mat[2, 3] = -0.02000f;
      proj_mat[3, 0] = +0.00000f; proj_mat[3, 1] = +0.00000f; proj_mat[3, 2] = -1.00000f; proj_mat[3, 3] = +0.00000f;
    }

    // use transform.worldToLocalMatrix, not Camera.worldToCameraMatrix
    var view_mat = this.cam.transform.worldToLocalMatrix;
    SetViewMat(this.camera_id, //
      view_mat[0, 0], view_mat[0, 1], view_mat[0, 2], view_mat[0, 3], //
      view_mat[1, 0], view_mat[1, 1], view_mat[1, 2], view_mat[1, 3], //
      view_mat[2, 0], view_mat[2, 1], view_mat[2, 2], view_mat[2, 3], //
      view_mat[3, 0], view_mat[3, 1], view_mat[3, 2], view_mat[3, 3]  //
    );
    this.cam.transform.rotation = q;
  }

  void OnPostRender()
  {
    GL.Clear(true, true, Color.clear);
    GL.IssuePluginEvent(GetRenderHandlerPtr(), this.camera_id);
  }


  void OnDestroy()
  {
    if (camera_id != 0)
    {
      UnregisterCamera(camera_id);
    }
  }

  bool captureRequested = false;
  public void RequestCapture()
  {
    this.captureRequested = true;
  }
  void OnRenderImage(RenderTexture src, RenderTexture dst)
  {
    Graphics.Blit(src, dst);
    if (this.captureRequested) try
      {
        this.captureRequested = false;
        var tex = new Texture2D(src.width, src.height);
        var prevActiveTex = RenderTexture.active;
        RenderTexture.active = src;
        tex.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
        tex.Apply();
        RenderTexture.active = prevActiveTex;

        string dst_dir;
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
          using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
          {
            using (var ctx = activity.Call<AndroidJavaObject>("getApplicationContext"))
            {
              using (var env = new AndroidJavaClass("android.os.Environment"))
              {
                using (var type = env.GetStatic<AndroidJavaObject>("DIRECTORY_PICTURES"))
                {
                  using (var path = env.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", type))
                  {
                    var path_str = path.Call<string>("toString");
                    dst_dir = $"{path_str}/screenshots";
                  }
                }
              }
            }
          }
        }
        if (!Directory.Exists(dst_dir))
        {
          Directory.CreateDirectory(dst_dir);
        }
        var filename = $"ZenMirror_{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.jpg";
        File.WriteAllBytes($"{dst_dir}/{filename}", tex.EncodeToJPG());
        AndroidToast.ShowToast($"'{filename}' is saved to {dst_dir}");
      }
      catch (Exception e)
      {
        AndroidToast.ShowToast($"Failed to save screenshot: {e.Message}");
      }
  }
}
