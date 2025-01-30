using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
  private TcpClient client = null;
  List<byte> data;

  void Start()
  {
    data = new List<byte> { };
    StartCoroutine(TryConnect());
  }

  bool isSearchingServer = false;
  IEnumerator TryConnect()
  {
    if (this.client != null && this.client.Connected)
    {
      this.isSearchingServer = false;
      yield break;
    }
    this.isSearchingServer = true;
    while (true)
    {
      Debug.Log("Searching for InputListenServer");
      try
      {
        this.client = new TcpClient("127.0.0.1", 22202);
        _ = ReadStream();
        break;
      }
      catch (Exception)
      {
        this.client = null;
      }
      yield return new WaitForSeconds(1);
    }
    Debug.Log("Connected");
    this.isSearchingServer = false;
  }
  async Task ReadStream()
  {
    var stream = this.client.GetStream();
    var buf = new Byte[256];
    while (this.client != null && this.client.Connected)
    {
      var readSize = await stream.ReadAsync(buf, 0, buf.Length);
      Debug.Log($"data read (len: {readSize})");
      if (readSize == 0)
      {
        Debug.Log("Disconnected");
        this.client.Close();
        this.client = null;
        StartCoroutine(TryConnect());
        break;
      }
    }
  }

  void SendData(UInt32 type, Byte[] data)
  {
    var stream = this.client.GetStream();
    var buf = new List<byte> {
      (byte)'y', (byte)'a', (byte)'z', (byte)'a',
      (byte)((type >> 0) & 0xFF),
      (byte)((type >> 8) & 0xFF),
      (byte)((type >> 16) & 0xFF),
      (byte)((type >> 24) & 0xFF),
    };
    buf.AddRange(data);
    stream.Write(buf.ToArray(), 0, buf.Count);
    stream.Flush();
  }
  void SendMouseDown()
  {
    SendData(2, BitConverter.GetBytes((UInt32)0));
  }
  void SendMouseUp()
  {
    SendData(3, BitConverter.GetBytes((UInt32)0));
  }

  void FixedUpdate()
  {
    if (this.client == null || !this.client.Connected)
    {
      if (!this.isSearchingServer)
      {
        StartCoroutine(TryConnect());
      }
      return;
    }
    try
    {
      HandleMouse();
      HandleTap();
    }
    catch (Exception e)
    {
      Debug.LogError($"Failed to send data: {e.Message}");
    }
  }
  void HandleMouse()
  {
    if (Input.touchCount == 0)
    {
      if (Input.GetMouseButtonDown(0))
      {
        SendMouseDown();
      }
      else if (Input.GetMouseButtonUp(0))
      {
        SendMouseUp();
      }
    }

    float x = Input.GetAxisRaw("Mouse X");
    float y = Input.GetAxisRaw("Mouse Y");
    if (x == 0.0F && y == 0.0F)
    {
      return;
    }
    data.AddRange(BitConverter.GetBytes(x));
    data.AddRange(BitConverter.GetBytes(y));
    SendData(1, data.ToArray());
    data.Clear();
  }

  enum TapStatus
  {
    Idle,
    Pending, // until TapThresholdSec passed from `Began` received
    MoveOnly, // down event is not sent
    TapSent,
  }
  TapStatus tapState = TapStatus.Idle;
  const float TapThresholdSec = 0.2F;
  float passedSecFromBegan = 0.0F;
  void HandleTap()
  {
    IEnumerator SendMouseUpAfterPassedSec()
    {
      yield return new WaitForSeconds(this.passedSecFromBegan);
      SendMouseUp();
    }

    var phase = TouchPhase.Ended;
    if (Input.touchCount > 0)
    {
      phase = Input.GetTouch(0).phase;
    }
    switch (this.tapState)
    {
      case TapStatus.Idle:
        if (phase == TouchPhase.Began)
        {
          this.tapState = TapStatus.Pending;
          this.passedSecFromBegan = 0.0F;
        }
        break;

      case TapStatus.Pending:
        switch (phase)
        {
          case TouchPhase.Moved:
            this.tapState = TapStatus.MoveOnly;
            break;
          case TouchPhase.Stationary:
            this.passedSecFromBegan += Time.deltaTime;
            if (this.passedSecFromBegan > TapThresholdSec) // Long tap (drag)
            {
              SendMouseDown();
              this.tapState = TapStatus.TapSent;
              Android.Vibrate();
            }
            break;
          case TouchPhase.Ended: // Tap duration is less than TapThreshold
            SendMouseDown();
            StartCoroutine(SendMouseUpAfterPassedSec());
            this.tapState = TapStatus.Idle;
            break;
          case TouchPhase.Canceled:
            this.tapState = TapStatus.Idle;
            break;
        }
        break;

      case TapStatus.MoveOnly:
        if (phase == TouchPhase.Canceled || phase == TouchPhase.Ended)
        {
          this.tapState = TapStatus.Idle;
        }
        break;
      case TapStatus.TapSent:
        if (phase == TouchPhase.Canceled)
        {
          this.tapState = TapStatus.Idle;
        }
        if (phase == TouchPhase.Ended)
        {
          SendMouseUp();
          this.tapState = TapStatus.Idle;
        }
        break;
    }
  }

  void OnDestroy()
  {
    Debug.Log("closing...");
    if (this.client != null && this.client.Connected)
    {
      this.client.Close();
    }
  }
}


