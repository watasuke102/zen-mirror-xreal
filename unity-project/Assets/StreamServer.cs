using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;

class Server : MonoBehaviour
{
  [SerializeField] OverrideCamera captureTarget;

  void Start()
  {
    Debug.Log("[WebSocket Server] Start()");
    StartCoroutine(main());
  }

  Texture2D tex = null;
  List<byte> current_frame = null;
  bool frame_updated = false;
  public void HandleGetCurrentFrame(AsyncGPUReadbackRequest req)
  {
    if (req.hasError)
    {
      Debug.LogWarning("Failed to get streaming image");
      return;
    }
    if (this.tex == null)
    {
      this.tex = new Texture2D(req.width, req.height, TextureFormat.RGBA32, false);
    }
    tex.LoadRawTextureData(req.GetData<Color32>());
    tex.Apply();
    var data = tex.EncodeToJPG(40);

    if (this.current_frame == null)
    {
      this.current_frame = new List<byte>();
    }
    else
    {
      this.current_frame.Clear();
    }
    this.current_frame.Add((1 << 7) | 0x02); // 0b<FIN, resv*3> _ <data is binary>

    // payload length; note that `mask` is not set
    if (data.Length <= 125)
    {
      this.current_frame.Add((byte)data.Length);
    }
    else if (data.Length <= UInt16.MaxValue)
    {
      this.current_frame.Add(126);
      var len = (UInt16)data.Length;
      this.current_frame.Add((byte)((len >> 8) & 0xff));
      this.current_frame.Add((byte)((len >> 0) & 0xff));
    }
    else
    {
      this.current_frame.Add(127);
      var len = (UInt64)data.Length;
      for (int i = 7; i >= 0; --i)
      {
        this.current_frame.Add((byte)((len >> (8 * i)) & 0xff));
      }
    }
    this.current_frame.AddRange(data);
    Debug.Log($"size: {this.current_frame.Count}");
    this.frame_updated = true;
  }

  // based on: https://developer.mozilla.org/ja/docs/Web/API/WebSockets_API/Writing_WebSocket_server
  IEnumerator main()
  {
    var server = new TcpListener(IPAddress.Parse("0.0.0.0"), 9999);
    server.Start();
    Debug.Log("WebSocket Server started (port: 9999)");
    while (true)
    {
      // wait client
      var clientAsync = server.AcceptTcpClientAsync();
      yield return new WaitUntil(() => clientAsync.IsCompleted);
      var client = clientAsync.Result;
      Debug.Log($"Client found: {client.Client.RemoteEndPoint}");
      // wait for handshake
      while (true)
      {
        NetworkStream stream = client.GetStream();
        yield return new WaitUntil(() => stream.DataAvailable);
        yield return new WaitUntil(() => client.Available >= 3); // "GET".length

        byte[] bytes = new byte[client.Available];
        var readAsync = stream.ReadAsync(bytes, 0, bytes.Length);
        yield return new WaitUntil(() => readAsync.IsCompleted);
        if (!readAsync.IsCompletedSuccessfully)
        {
          Debug.LogError($"Failed to read data: {readAsync.Exception.Message}");
          continue;
        }
        string s = Encoding.UTF8.GetString(bytes);

        if (!Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
        {
          continue;
        }
        // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
        // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
        // 3. Compute SHA-1 and Base64 hash of the new value
        // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
        string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
        string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
        string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

        // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
        byte[] response = Encoding.UTF8.GetBytes(
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Connection: Upgrade\r\n" +
            "Upgrade: websocket\r\n" +
            "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

        var writeAsync = stream.WriteAsync(response, 0, response.Length);
        yield return new WaitUntil(() => writeAsync.IsCompleted);
        if (writeAsync.IsCompletedSuccessfully)
        {
          break;
        }
        Debug.LogWarning($"Failed to respond handshake request: {writeAsync.Exception.Message}");
      }

      Debug.Log("Handshake is completed");
      captureTarget.StartStreaming();
      while (true)
      {
        if (!client.Connected)
        {
          Debug.Log("client.Connected is false");
          break;
        }
        yield return new WaitForEndOfFrame();
        NetworkStream stream = client.GetStream();
        if (!stream.CanWrite)
        {
          Debug.LogWarning("Writing to stream is disabled?");
        }
        else
        if (this.frame_updated)
        {
          var writeAsync = stream.WriteAsync(this.current_frame.ToArray(), 0, this.current_frame.Count);
          this.frame_updated = false;
          yield return new WaitUntil(() => writeAsync.IsCompleted);
          if (!writeAsync.IsCompletedSuccessfully)
          {
            Debug.LogError($"Failed to send image: {writeAsync.Exception.Message}");
            // break;
          }
        }
        // read client data
        if (!stream.DataAvailable)
        {
          continue;
        }
        byte[] bytes = new byte[client.Available];
        var readAsync = stream.ReadAsync(bytes, 0, bytes.Length);
        yield return new WaitUntil(() => readAsync.IsCompleted);
        if (!readAsync.IsCompletedSuccessfully)
        {
          Debug.LogError($"Failed to read data: {readAsync.Exception.Message}");
          break;
        }
        bool fin = (bytes[0] & 0b1000_0000) != 0,
             mask = (bytes[1] & 0b1000_0000) != 0; // must be true, "All messages from the client to the server have this bit set"
        int opcode = bytes[0] & 0b0000_1111; // expecting 1 - text message
        ulong offset = 2,
              msglen = (ulong)(bytes[1] & 0b0111_1111);

        if (msglen == 126)
        {
          // bytes are reversed because websocket will print them in Big-Endian, whereas
          // BitConverter will want them arranged in little-endian on windows
          msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
          offset = 4;
        }
        else if (msglen == 127)
        {
          // To test the below code, we need to manually buffer larger messages — since the NIC's autobuffering
          // may be too latency-friendly for this code to run (that is, we may have only some of the bytes in this
          // websocket frame available through client.Available).
          msglen = BitConverter.ToUInt64(new byte[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] }, 0);
          offset = 10;
        }

        if (msglen == 0)
        {
          Debug.LogWarning("msglen == 0");
        }
        else if (mask)
        {
          byte[] decoded = new byte[msglen];
          byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
          offset += 4;

          for (ulong i = 0; i < msglen; ++i)
            decoded[i] = (byte)(bytes[offset + (ulong)i] ^ masks[i % 4]);

          string text = Encoding.UTF8.GetString(decoded);
          Debug.Log($"mes: {text}");
          if (opcode == 0x09)
          {
            Debug.Log("opcode is connection close");
            break;
          }
          else if (opcode == 0x09)
          {
            Debug.Log("Ping received!");
            // received ping, so return pong
            bytes[0] = (1 << 7) | 0x0a;
            var writeAsync = stream.WriteAsync(bytes, 0, bytes.Length);
            yield return new WaitUntil(() => writeAsync.IsCompleted);
            if (!writeAsync.IsCompletedSuccessfully)
            {
              Debug.LogWarning($"Failed to return pong: {writeAsync.Exception.Message}");
              break;
            }
          }
        }
        else
        {
          Debug.LogWarning("mask bit not set");
        }
      }
      if (client.Connected)
      {
        client.Close(); // In fact, close sequense is needed
      }
      Debug.Log("Disconnected");
      this.captureTarget.StopStreaming();
    } // end of main loop
  }
}
