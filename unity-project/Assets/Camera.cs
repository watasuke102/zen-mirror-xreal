using System;
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
  private static extern void SetViewProjectionMat(Int32 camera_id,//
      float v_x0, float v_x1, float v_x2, float v_x3,  //
      float v_y0, float v_y1, float v_y2, float v_y3,  //
      float v_z0, float v_z1, float v_z2, float v_z3,  //
      float v_w0, float v_w1, float v_w2, float v_w3,  //
      float p_x0, float p_x1, float p_x2, float p_x3,  //
      float p_y0, float p_y1, float p_y2, float p_y3,  //
      float p_z0, float p_z1, float p_z2, float p_z3,  //
      float p_w0, float p_w1, float p_w2, float p_w3   //
  );

  void Start()
  {
    this.cam = GetComponent<Camera>();
    var width = this.cam.pixelWidth;
    var height = this.cam.pixelHeight;
    this.camera_id = RegisterCamera(width, height);
    Debug.Log($"[New Camera] {this.cam.name}: pixel={width}x{height}");
  }
  void Update()
  {
    this.cam.transform.position = new Vector3(0.0f, 0.8f, 0.0f);
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

    var view_mat = this.cam.worldToCameraMatrix;
    SetViewProjectionMat(this.camera_id, //
      view_mat[0, 0], view_mat[0, 1], view_mat[0, 2], view_mat[0, 3], //
      view_mat[1, 0], view_mat[1, 1], view_mat[1, 2], view_mat[1, 3], //
      view_mat[2, 0], view_mat[2, 1], view_mat[2, 2], view_mat[2, 3], //
      view_mat[3, 0], view_mat[3, 1], view_mat[3, 2], view_mat[3, 3],  //
      proj_mat[0, 0], proj_mat[0, 1], proj_mat[0, 2], proj_mat[0, 3], //
      proj_mat[1, 0], proj_mat[1, 1], proj_mat[1, 2], proj_mat[1, 3], //
      proj_mat[2, 0], proj_mat[2, 1], proj_mat[2, 2], proj_mat[2, 3], //
      proj_mat[3, 0], proj_mat[3, 1], proj_mat[3, 2], proj_mat[3, 3]  //
    );
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
}
