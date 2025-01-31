using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
  [SerializeField] InputField addressInput;
  private TcpClient client = null;
  // private string clientAddress = "127.0.0.1";
  private string clientAddress = "192.168.0.9";
  List<byte> data;

  public void SetAddress(InputField input)
  {
    this.clientAddress = input.text;
  }

  void Start()
  {
    data = new List<byte> { };
    this.addressInput.text = this.clientAddress;
    this.addressInput.onEndEdit.AddListener(delegate { this.SetAddress(this.addressInput); });
    StartCoroutine(TryConnect());
  }
  void OnDestroy()
  {
    Debug.Log("closing...");
    if (this.client != null && this.client.Connected)
    {
      this.client.Close();
    }
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
      Debug.Log($"Searching for InputListenServer at {this.clientAddress}:22202");
      if (this.client == null)
      {
        this.client = new TcpClient();
      }
      var task = this.client.ConnectAsync(this.clientAddress, 22202);
      var timeout = Task.Delay(500);
      yield return new WaitUntil(() => task.IsCompleted || timeout.IsCompleted);
      if (task.IsCompletedSuccessfully)
      {
        break;
      }
      yield return new WaitForSeconds(0.5F);
    }
    _ = ReadStream();
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
  void SendMouseMove(float x, float y)
  {
    data.AddRange(BitConverter.GetBytes(x));
    data.AddRange(BitConverter.GetBytes(y));
    SendData(1, data.ToArray());
    data.Clear();
  }

  enum MouseButton
  {
    Left = 0x110,
    Right = 0x111,
  }
  void SendMouseDown(MouseButton b)
  {
    SendData(2, BitConverter.GetBytes((UInt32)b));
  }
  void SendMouseUp(MouseButton b)
  {
    SendData(3, BitConverter.GetBytes((UInt32)b));
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
    if (Input.touchCount != 0)
    {
      return;
    }
    if (Input.GetMouseButtonDown(0))
    {
      SendMouseDown(MouseButton.Left);
    }
    else if (Input.GetMouseButtonUp(0))
    {
      SendMouseUp(MouseButton.Left);
    }
    if (Input.GetMouseButtonDown(1))
    {
      SendMouseDown(MouseButton.Right);
    }
    else if (Input.GetMouseButtonUp(1))
    {
      SendMouseUp(MouseButton.Right);
    }

    float x = Input.GetAxisRaw("Mouse X");
    float y = Input.GetAxisRaw("Mouse Y");
    if (x != 0.0F || y != 0.0F)
    {
      SendMouseMove(x, y);
    }
  }

  enum TapStatus
  {
    Idle,
    Pending, // until TapThresholdSec passed from `Began` received
    LongTap,
    MoveOnly, // down event is not sent at the begenning
    Drag,
    Scroll,
  }
  TapStatus tapState = TapStatus.Idle;
  const float TapThresholdSec = 0.2F;
  float passedSecFromBegan = 0.0F;
  void HandleTap()
  {
    IEnumerator SendMouseUpAfterDelay(MouseButton b, float delay)
    {
      yield return new WaitForSeconds(delay);
      SendMouseUp(b);
    }

    var phase = TouchPhase.Ended;
    if (Input.touchCount > 0)
    {
      var touch = Input.GetTouch(0);
      phase = touch.phase;
      if (phase == TouchPhase.Moved && touch.deltaPosition.sqrMagnitude <= 2.0F)
      {
        phase = TouchPhase.Stationary; // discard small movement
      }
    }
    if (phase == TouchPhase.Canceled)
    {
      phase = TouchPhase.Ended;
    }

    switch (this.tapState)
    {
      case TapStatus.Idle:
        if (phase == TouchPhase.Began)
        {
          this.tapState = TapStatus.Pending;
          this.passedSecFromBegan = 0.0F;
        }
        if (phase == TouchPhase.Moved)
        {
          this.tapState = TapStatus.MoveOnly;
        }
        break;

      case TapStatus.Pending:
        if (Input.touchCount >= 2)
        {
          this.tapState = TapStatus.Scroll;
          break;
        }
        switch (phase)
        {
          case TouchPhase.Moved:
            this.tapState = TapStatus.MoveOnly;
            break;
          case TouchPhase.Stationary:
            this.passedSecFromBegan += Time.deltaTime;
            if (this.passedSecFromBegan > TapThresholdSec) // Long tap
            {
              this.tapState = TapStatus.LongTap;
              Android.Vibrate();
            }
            break;
          case TouchPhase.Ended: // Tap duration is less than TapThreshold
            SendMouseDown(MouseButton.Left);
            StartCoroutine(SendMouseUpAfterDelay(MouseButton.Left, this.passedSecFromBegan));
            this.tapState = TapStatus.Idle;
            break;
        }
        break;

      case TapStatus.LongTap:
        if (phase == TouchPhase.Moved)
        {
          SendMouseDown(MouseButton.Left); // start dragging
          this.tapState = TapStatus.Drag;
        }
        else if (phase == TouchPhase.Ended)
        {
          SendMouseDown(MouseButton.Right);
          SendMouseUpAfterDelay(MouseButton.Right, 0.2F);
          this.tapState = TapStatus.Idle;
        }
        else if (Input.touchCount >= 2)
        {
          this.tapState = TapStatus.Scroll;
        }
        break;

      case TapStatus.MoveOnly:
        if (phase == TouchPhase.Moved)
        {
          var v = Input.GetTouch(0).deltaPosition;
          SendMouseMove(v.x, v.y);
        }
        else if (phase == TouchPhase.Ended)
        {
          this.tapState = TapStatus.Idle;
        }
        else if (Input.touchCount >= 2)
        {
          this.tapState = TapStatus.Scroll;
        }
        break;
      case TapStatus.Drag:
        if (phase == TouchPhase.Moved)
        {
          var v = Input.GetTouch(0).deltaPosition;
          SendMouseMove(v.x, v.y);
        }
        if (phase == TouchPhase.Ended)
        {
          SendMouseUp(MouseButton.Left);
          this.tapState = TapStatus.Idle;
        }
        break;

      case TapStatus.Scroll:
        if (phase == TouchPhase.Moved)
        {
          var delta = Input.GetTouch(0).deltaPosition;
          SendData(4, BitConverter.GetBytes(delta.sqrMagnitude * (delta.y < 0 ? -1.0F : 1.0F)));
        }
        else if (Input.touchCount < 2)
        {
          this.tapState = TapStatus.Idle;
        }
        break;
    }
  }
}


