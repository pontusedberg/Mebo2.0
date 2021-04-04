// Decompiled with JetBrains decompiler
// Type: Mebo2UdpComms
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1844E21B-8F26-4849-82BD-78183271FBE6
// Assembly location: C:\Users\Gedyy\Desktop\tttt\assets\bin\Data\Managed\Assembly-CSharp.dll

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Mebo2UdpComms : MonoBehaviour
{
  public int sendFrequencyHz = 10;
  private bool doesSend = true;
  private AutoResetEvent stopUdpThreadEvent = new AutoResetEvent(false);
  public Action onCommandSent;
  private bool commsAreOpen;
  private Thread udpThread;
  private IPEndPoint udpIPEP;
  private Socket udpSocket;
  private static byte[] latestRCommand;

  private void Start()
  {
    this.udpIPEP = new IPEndPoint(IPAddress.Parse("192.168.99.1"), 13099);
    this.OpenComms();
  }

  private void OnDestroy()
  {
    this.CloseComms();
  }

  private void OnApplicationPause(bool isPaused)
  {
    this.doesSend = !isPaused;
    if (isPaused)
      this.CloseComms();
    else
      this.OpenComms();
  }

  private void OpenComms()
  {
    Debug.Log((object) ("OpenComms commsAreOpen=" + (object) this.commsAreOpen));
    if (this.commsAreOpen)
      return;
    this.udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    this.stopUdpThreadEvent.Reset();
    this.udpThread = new Thread(new ThreadStart(this.UdpThread))
    {
      IsBackground = true
    };
    this.udpThread.Start();
    this.commsAreOpen = true;
  }

  private void CloseComms()
  {
    Debug.Log((object) ("CloseComms commsAreOpen=" + (object) this.commsAreOpen));
    if (!this.commsAreOpen)
      return;
    if (this.udpThread != null && this.udpThread.IsAlive)
    {
      Debug.Log((object) "stopUdpThreadEvent being set now");
      this.stopUdpThreadEvent.Set();
      if (!this.udpThread.Join(5))
      {
        Debug.Log((object) "udpThread !threadDidTerminate - aborting");
        this.udpThread.Abort();
      }
    }
    if (this.udpSocket != null)
    {
      this.udpSocket.Close();
      this.udpSocket = (Socket) null;
    }
    Mebo2UdpComms.latestRCommand = (byte[]) null;
    this.commsAreOpen = false;
  }

  public static void UpdateRCommand(string cmd)
  {
    if (cmd == null)
      Mebo2UdpComms.latestRCommand = (byte[]) null;
    else
      Mebo2UdpComms.latestRCommand = Encoding.ASCII.GetBytes(cmd);
  }

  private void UdpThread()
  {
    Debug.Log((object) "Mebo2UdpComms Entering thread");
    TimeSpan timeSpan = TimeSpan.FromSeconds(1.0 / (double) this.sendFrequencyHz);
    int millisecondsTimeout;
    do
    {
      DateTime dateTime = DateTime.Now + timeSpan;
      if (Mebo2UdpComms.latestRCommand != null && this.doesSend)
      {
        this.udpSocket.SendTo(Mebo2UdpComms.latestRCommand, (EndPoint) this.udpIPEP);
        if (this.onCommandSent != null)
          this.onCommandSent();
      }
      millisecondsTimeout = (int) (dateTime - DateTime.Now).TotalMilliseconds;
      if (millisecondsTimeout < 10)
        millisecondsTimeout = 10;
    }
    while (!this.stopUdpThreadEvent.WaitOne(millisecondsTimeout));
    Debug.Log((object) "Mebo2UdpComms Exiting thread");
  }
}
