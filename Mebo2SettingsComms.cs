// Decompiled with JetBrains decompiler
// Type: Mebo2SettingsComms
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1844E21B-8F26-4849-82BD-78183271FBE6
// Assembly location: C:\Users\Gedyy\Desktop\tttt\assets\bin\Data\Managed\Assembly-CSharp.dll

using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Mebo2SettingsComms : MonoBehaviour
{
  public static int batteryLevel = 0;
  private static Queue<Mebo2SettingsComms.Request> requestQueue = new Queue<Mebo2SettingsComms.Request>();
  private static Queue<Mebo2SettingsComms.Request> responseQueue = new Queue<Mebo2SettingsComms.Request>();
  public static Action<string, string> onGetVersionsComplete;
  public static Action<bool, int> onGetBatteryLevelComplete;
  public static Action<bool, bool> onGetEyeLedStateComplete;
  public static Action<bool, bool> onEyeLedToggleComplete;
  public static Action<bool, bool> onGetClawLedStateComplete;
  public static Action<bool, bool> onClawLedToggleComplete;
  public static Action<string, string, Mebo2SettingsComms.EWiFiSecurity, int> onGetSsidComplete;
  public static Action<bool> onSetSsidComplete;
  public static Action onReboot;
  private static Mebo2SettingsComms instance;
  private Coroutine updateRequestsCoroutine;

  public static bool Started { get; private set; }

  public static event Mebo2SettingsComms.OnTimeoutURLDelegate OnTimeout = _param0 => {};

  private void StartRequestUpdates()
  {
    if (this.updateRequestsCoroutine != null)
      return;
    this.updateRequestsCoroutine = this.StartCoroutine(this.UpdateRequests());
  }

  private void StopRequestUpdates()
  {
    if (this.updateRequestsCoroutine == null)
      return;
    this.StopCoroutine(this.updateRequestsCoroutine);
    this.updateRequestsCoroutine = (Coroutine) null;
  }

  public static void StartComms()
  {
    UnityEngine.Debug.Log((object) "Mebo 2 Seetings Comms Started!".ToBold().Color(Color.cyan));
    if ((bool) ((UnityEngine.Object) Mebo2SettingsComms.instance))
      Mebo2SettingsComms.instance.StartRequestUpdates();
    Mebo2SettingsComms.Started = true;
  }

  public static void StopComms()
  {
    UnityEngine.Debug.Log((object) "Mebo 2 Seetings Comms Stopped!".ToBold().Color(Color.magenta));
    if ((bool) ((UnityEngine.Object) Mebo2SettingsComms.instance))
      Mebo2SettingsComms.instance.StopRequestUpdates();
    Mebo2SettingsComms.Started = false;
  }

  private void Awake()
  {
    Mebo2SettingsComms.instance = this;
  }

  private void OnDestory()
  {
    Mebo2SettingsComms.instance = (Mebo2SettingsComms) null;
  }

  private void Update()
  {
    this.UpdateResponses();
  }

  private void UpdateResponses()
  {
    if (!Mebo2SettingsComms.Started)
      return;
    while (true)
    {
      Mebo2SettingsComms.Request request = (Mebo2SettingsComms.Request) null;
      if (Mebo2SettingsComms.responseQueue.Count > 0)
        request = Mebo2SettingsComms.responseQueue.Dequeue();
      if (request != null)
      {
        try
        {
          request.HandleResponse();
        }
        catch (Exception ex)
        {
          UnityEngine.Debug.LogWarningFormat("HandleResponse threw {0}", (object) ex);
        }
      }
      else
        break;
    }
  }

  public static void RequestGetVersions()
  {
    string urlToRequest = "?command1=sonix_version()&command2=mebolink_message_send(VER=?)";
    // ISSUE: reference to a compiler-generated field
    if (Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache0 == null)
    {
      // ISSUE: reference to a compiler-generated field
      Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache0 = new Mebo2SettingsComms.HandleResponseDelegate(Mebo2SettingsComms.HandleGetVersionsResponse);
    }
    // ISSUE: reference to a compiler-generated field
    Mebo2SettingsComms.HandleResponseDelegate fMgCache0 = Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache0;
    int timeoutMilliseconds = 0;
    int num = 0;
    Mebo2SettingsComms.SendRequest(urlToRequest, fMgCache0, timeoutMilliseconds, (MeboTags) num);
  }

  public static void RequestGetVersions(int timeout)
  {
    string urlToRequest = "?command1=sonix_version()&command2=mebolink_message_send(VER=?)";
    // ISSUE: reference to a compiler-generated field
    if (Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache1 == null)
    {
      // ISSUE: reference to a compiler-generated field
      Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache1 = new Mebo2SettingsComms.HandleResponseDelegate(Mebo2SettingsComms.HandleGetVersionsResponse);
    }
    // ISSUE: reference to a compiler-generated field
    Mebo2SettingsComms.HandleResponseDelegate fMgCache1 = Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache1;
    int timeoutMilliseconds = timeout;
    int num = 0;
    Mebo2SettingsComms.SendRequest(urlToRequest, fMgCache1, timeoutMilliseconds, (MeboTags) num);
  }

  private static void HandleGetVersionsResponse(string responseString)
  {
    string str1 = (string) null;
    string str2 = (string) null;
    object[] objArray = (object[]) null;
    try
    {
      objArray = Json.DeserializeMultiple(responseString);
      str1 = (objArray[0] as Dictionary<string, object>)["sonix_svn_revision"] as string;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogWarning((object) ("Unable to parse Sonix firmware SVN revision " + (object) ex));
    }
    try
    {
      string str3 = (objArray[1] as Dictionary<string, object>)["response"] as string;
      if (!str3.StartsWith("VER="))
        throw new Exception("!motorBoardVersionResponseString.StartsWith(\"VER=\")");
      str2 = str3.Substring("VER=".Length);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogWarning((object) ("Unable to parse motor board firmware SVN revision " + (object) ex));
    }
    if (Mebo2SettingsComms.onGetVersionsComplete == null)
      return;
    Mebo2SettingsComms.onGetVersionsComplete(str1, str2);
  }

  public static void RequestGetBatteryLevel()
  {
    string urlToRequest = "?command1=mebolink_message_send(BAT=?)";
    // ISSUE: reference to a compiler-generated field
    if (Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache2 == null)
    {
      // ISSUE: reference to a compiler-generated field
      Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache2 = new Mebo2SettingsComms.HandleResponseDelegate(Mebo2SettingsComms.HandleGetBatteryLevelResponse);
    }
    // ISSUE: reference to a compiler-generated field
    Mebo2SettingsComms.HandleResponseDelegate fMgCache2 = Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache2;
    int timeoutMilliseconds = 0;
    int num = 0;
    Mebo2SettingsComms.SendRequest(urlToRequest, fMgCache2, timeoutMilliseconds, (MeboTags) num);
  }

  private static void HandleGetBatteryLevelResponse(string responseString)
  {
    bool flag = false;
    Mebo2SettingsComms.batteryLevel = 0;
    try
    {
      string str = (Json.Deserialize(responseString) as Dictionary<string, object>)["response"] as string;
      if (!str.StartsWith("BAT="))
        throw new Exception("!batteryResponseString.StartsWith(\"BAT=\")");
      Mebo2SettingsComms.batteryLevel = int.Parse(str.Substring("BAT=".Length));
      flag = true;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogWarning((object) ("Unable to parse battery level " + (object) ex));
    }
    if (Mebo2SettingsComms.onGetBatteryLevelComplete == null)
      return;
    Mebo2SettingsComms.onGetBatteryLevelComplete(flag, Mebo2SettingsComms.batteryLevel);
  }

  public static void RequestGetEyeLedState()
  {
    string urlToRequest = "?command1=eye_led_state()";
    // ISSUE: reference to a compiler-generated field
    if (Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache3 == null)
    {
      // ISSUE: reference to a compiler-generated field
      Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache3 = new Mebo2SettingsComms.HandleResponseDelegate(Mebo2SettingsComms.HandleGetEyeLedStateResponse);
    }
    // ISSUE: reference to a compiler-generated field
    Mebo2SettingsComms.HandleResponseDelegate fMgCache3 = Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache3;
    int timeoutMilliseconds = 0;
    int num = 0;
    Mebo2SettingsComms.SendRequest(urlToRequest, fMgCache3, timeoutMilliseconds, (MeboTags) num);
  }

  private static void HandleGetEyeLedStateResponse(string responseString)
  {
    bool ledIsOn;
    bool fromResponseString = Mebo2SettingsComms.GetLedStateFromResponseString(responseString, out ledIsOn);
    if (Mebo2SettingsComms.onGetEyeLedStateComplete == null)
      return;
    Mebo2SettingsComms.onGetEyeLedStateComplete(fromResponseString, ledIsOn);
  }

  public static void RequestEyeLedToggle()
  {
    string urlToRequest = "?command1=eye_led_toggle()";
    // ISSUE: reference to a compiler-generated field
    if (Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache4 == null)
    {
      // ISSUE: reference to a compiler-generated field
      Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache4 = new Mebo2SettingsComms.HandleResponseDelegate(Mebo2SettingsComms.HandleEyeLedToggleResponse);
    }
    // ISSUE: reference to a compiler-generated field
    Mebo2SettingsComms.HandleResponseDelegate fMgCache4 = Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache4;
    int timeoutMilliseconds = 0;
    int num = 0;
    Mebo2SettingsComms.SendRequest(urlToRequest, fMgCache4, timeoutMilliseconds, (MeboTags) num);
  }

  private static void HandleEyeLedToggleResponse(string responseString)
  {
    bool ledIsOn;
    bool fromResponseString = Mebo2SettingsComms.GetLedStateFromResponseString(responseString, out ledIsOn);
    if (Mebo2SettingsComms.onEyeLedToggleComplete == null)
      return;
    Mebo2SettingsComms.onEyeLedToggleComplete(fromResponseString, ledIsOn);
  }

  public static void RequestGetClawLedState()
  {
    string urlToRequest = "?command1=claw_led_state()";
    // ISSUE: reference to a compiler-generated field
    if (Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache5 == null)
    {
      // ISSUE: reference to a compiler-generated field
      Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache5 = new Mebo2SettingsComms.HandleResponseDelegate(Mebo2SettingsComms.HandleGetClawLedStateResponse);
    }
    // ISSUE: reference to a compiler-generated field
    Mebo2SettingsComms.HandleResponseDelegate fMgCache5 = Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache5;
    int timeoutMilliseconds = 0;
    int num = 0;
    Mebo2SettingsComms.SendRequest(urlToRequest, fMgCache5, timeoutMilliseconds, (MeboTags) num);
  }

  private static void HandleGetClawLedStateResponse(string responseString)
  {
    bool ledIsOn;
    bool fromResponseString = Mebo2SettingsComms.GetLedStateFromResponseString(responseString, out ledIsOn);
    if (Mebo2SettingsComms.onGetClawLedStateComplete == null)
      return;
    Mebo2SettingsComms.onGetClawLedStateComplete(fromResponseString, ledIsOn);
  }

  public static void RequestClawLedToggle()
  {
    string urlToRequest = "?command1=claw_led_toggle()";
    // ISSUE: reference to a compiler-generated field
    if (Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache6 == null)
    {
      // ISSUE: reference to a compiler-generated field
      Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache6 = new Mebo2SettingsComms.HandleResponseDelegate(Mebo2SettingsComms.HandleClawLedToggleResponse);
    }
    // ISSUE: reference to a compiler-generated field
    Mebo2SettingsComms.HandleResponseDelegate fMgCache6 = Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache6;
    int timeoutMilliseconds = 0;
    int num = 0;
    Mebo2SettingsComms.SendRequest(urlToRequest, fMgCache6, timeoutMilliseconds, (MeboTags) num);
  }

  private static void HandleClawLedToggleResponse(string responseString)
  {
    bool ledIsOn;
    bool fromResponseString = Mebo2SettingsComms.GetLedStateFromResponseString(responseString, out ledIsOn);
    if (Mebo2SettingsComms.onClawLedToggleComplete == null)
      return;
    Mebo2SettingsComms.onClawLedToggleComplete(fromResponseString, ledIsOn);
  }

  public static void RequestGetSsid()
  {
    string urlToRequest = "?command1=get_ssid()";
    // ISSUE: reference to a compiler-generated field
    if (Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache7 == null)
    {
      // ISSUE: reference to a compiler-generated field
      Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache7 = new Mebo2SettingsComms.HandleResponseDelegate(Mebo2SettingsComms.HandleGetSsidResponse);
    }
    // ISSUE: reference to a compiler-generated field
    Mebo2SettingsComms.HandleResponseDelegate fMgCache7 = Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache7;
    int timeoutMilliseconds = 0;
    int num = 0;
    Mebo2SettingsComms.SendRequest(urlToRequest, fMgCache7, timeoutMilliseconds, (MeboTags) num);
  }

  private static void HandleGetSsidResponse(string responseString)
  {
    UnityEngine.Debug.Log((object) responseString.Color(Color.red));
    string str1 = (string) null;
    string str2 = (string) null;
    int num1 = 0;
    Mebo2SettingsComms.EWiFiSecurity ewiFiSecurity1 = Mebo2SettingsComms.EWiFiSecurity.Unknown;
    try
    {
      Dictionary<string, object> dictionary = Json.Deserialize(responseString) as Dictionary<string, object>;
      string str3 = dictionary["ssid"] as string;
      string str4 = dictionary["password"] as string;
      string key = dictionary["authmode"].ToString();
      if (key != null)
      {
        // ISSUE: reference to a compiler-generated field
        if (Mebo2SettingsComms.\u003C\u003Ef__switch\u0024map3 == null)
        {
          // ISSUE: reference to a compiler-generated field
          Mebo2SettingsComms.\u003C\u003Ef__switch\u0024map3 = new Dictionary<string, int>(8)
          {
            {
              "0",
              0
            },
            {
              "OPEN",
              0
            },
            {
              "1",
              1
            },
            {
              "WEP",
              1
            },
            {
              "2",
              2
            },
            {
              "WPA",
              2
            },
            {
              "3",
              3
            },
            {
              "WPA2",
              3
            }
          };
        }
        int num2;
        // ISSUE: reference to a compiler-generated field
        if (Mebo2SettingsComms.\u003C\u003Ef__switch\u0024map3.TryGetValue(key, out num2))
        {
          Mebo2SettingsComms.EWiFiSecurity ewiFiSecurity2;
          switch (num2)
          {
            case 0:
              ewiFiSecurity2 = Mebo2SettingsComms.EWiFiSecurity.Open;
              break;
            case 1:
              ewiFiSecurity2 = Mebo2SettingsComms.EWiFiSecurity.Wep;
              break;
            case 2:
              ewiFiSecurity2 = Mebo2SettingsComms.EWiFiSecurity.Wpa;
              break;
            case 3:
              ewiFiSecurity2 = Mebo2SettingsComms.EWiFiSecurity.Wpa2;
              break;
            default:
              goto label_11;
          }
          int num3 = (int) (long) dictionary["channel"];
          if (num3 < 0 || num3 > 14)
            throw new Exception("Invalid retrievedChannel");
          str1 = str3;
          str2 = str4;
          num1 = num3;
          ewiFiSecurity1 = ewiFiSecurity2;
          goto label_16;
        }
      }
label_11:
      throw new Exception("Invalid retrievedAuthorisationModeIdx");
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("Unable to parse get ssid response " + (object) ex));
    }
label_16:
    if (Mebo2SettingsComms.onGetSsidComplete == null)
      return;
    Mebo2SettingsComms.onGetSsidComplete(str1, str2, ewiFiSecurity1, num1);
  }

  public static void RequestSetSsid(string ssid, string password, Mebo2SettingsComms.EWiFiSecurity wiFiCert, int channel)
  {
    int num1;
    switch (wiFiCert)
    {
      case Mebo2SettingsComms.EWiFiSecurity.Wep:
        num1 = 1;
        break;
      case Mebo2SettingsComms.EWiFiSecurity.Wpa:
        num1 = 2;
        break;
      case Mebo2SettingsComms.EWiFiSecurity.Wpa2:
        num1 = 3;
        break;
      default:
        num1 = 0;
        break;
    }
    string str = string.Format("?command1=set_ssid({0},{1},{2},{3})", (object) ssid, (object) password, (object) num1, (object) channel);
    UnityEngine.Debug.Log((object) ("SET SSID : \n" + str));
    string urlToRequest = str;
    // ISSUE: reference to a compiler-generated field
    if (Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache8 == null)
    {
      // ISSUE: reference to a compiler-generated field
      Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache8 = new Mebo2SettingsComms.HandleResponseDelegate(Mebo2SettingsComms.HandleSetSsidResponse);
    }
    // ISSUE: reference to a compiler-generated field
    Mebo2SettingsComms.HandleResponseDelegate fMgCache8 = Mebo2SettingsComms.\u003C\u003Ef__mg\u0024cache8;
    int timeoutMilliseconds = 0;
    int num2 = 0;
    Mebo2SettingsComms.SendRequest(urlToRequest, fMgCache8, timeoutMilliseconds, (MeboTags) num2);
  }

  private static void HandleSetSsidResponse(string responseString)
  {
    string[] strArray = new string[2]
    {
      "Set SSID and password\n",
      "Set password\n"
    };
    bool flag = false;
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (responseString == strArray[index])
      {
        flag = true;
        break;
      }
    }
    UnityEngine.Debug.LogFormat("HandleSetSsidResponse \"{0}\"", (object) responseString);
    if (Mebo2SettingsComms.onSetSsidComplete == null)
      return;
    Mebo2SettingsComms.onSetSsidComplete(flag);
  }

  public static void RequestReboot()
  {
    Mebo2SettingsComms.SendRequest(new Mebo2SettingsComms.Request("?command1=reboot_companion()", (Mebo2SettingsComms.HandleResponseDelegate) null, 1000, MeboTags.none)
    {
      IsRebootCommand = true
    });
  }

  public static void SendRequest(string urlToRequest, Mebo2SettingsComms.HandleResponseDelegate responseHandler = null, int timeoutMilliseconds = 0, MeboTags tag = MeboTags.none)
  {
    Mebo2SettingsComms.SendRequest(new Mebo2SettingsComms.Request(urlToRequest, responseHandler, timeoutMilliseconds, tag));
  }

  private static void SendRequest(Mebo2SettingsComms.Request request)
  {
    Mebo2SettingsComms.requestQueue.Enqueue(request);
  }

  public static void FlushQueue(MeboTags tag)
  {
    Mebo2SettingsComms.Request[] array = Mebo2SettingsComms.requestQueue.ToArray();
    Mebo2SettingsComms.requestQueue.Clear();
    for (int index = 0; index < array.Length; ++index)
    {
      if (tag == MeboTags.none)
        Mebo2SettingsComms.requestQueue.Enqueue(array[index]);
      else if ((array[index].Tag & tag) != tag)
        Mebo2SettingsComms.requestQueue.Enqueue(array[index]);
    }
  }

  [DebuggerHidden]
  private IEnumerator UpdateRequests()
  {
    // ISSUE: object of a compiler-generated type is created
    return (IEnumerator) new Mebo2SettingsComms.\u003CUpdateRequests\u003Ec__Iterator0()
    {
      \u0024this = this
    };
  }

  [DebuggerHidden]
  private IEnumerator PerformRequestSend(Mebo2SettingsComms.Request requestToSend)
  {
    // ISSUE: object of a compiler-generated type is created
    return (IEnumerator) new Mebo2SettingsComms.\u003CPerformRequestSend\u003Ec__Iterator1()
    {
      requestToSend = requestToSend
    };
  }

  private static bool GetLedStateFromResponseString(string responseString, out bool ledIsOn)
  {
    bool flag = false;
    ledIsOn = false;
    try
    {
      string str = (Json.Deserialize(responseString) as Dictionary<string, object>)["response"] as string;
      if (str.StartsWith("ON"))
      {
        flag = true;
        ledIsOn = true;
      }
      else if (str.StartsWith("OFF"))
      {
        flag = true;
        ledIsOn = false;
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogWarning((object) ("Unable to parse LED state response " + (object) ex));
    }
    return flag;
  }

  public delegate void OnTimeoutURLDelegate(string url);

  public enum EWiFiSecurity
  {
    Unknown,
    Open,
    None,
    Wep,
    Wpa,
    Wpa2,
  }

  public delegate void HandleResponseDelegate(string responseString);

  private class Request
  {
    public string url;
    private Mebo2SettingsComms.HandleResponseDelegate handleResponseDelegate;
    public string responseString;
    public int timeoutMilliseconds;
    public MeboTags Tag;
    public bool IsRebootCommand;

    public Request(string url, Mebo2SettingsComms.HandleResponseDelegate handleResponseDelegate, int timeoutMilliseconds, MeboTags tag = MeboTags.none)
    {
      this.url = url;
      this.handleResponseDelegate = handleResponseDelegate;
      this.timeoutMilliseconds = timeoutMilliseconds;
      this.Tag = tag;
    }

    public void HandleResponse()
    {
      if (this.handleResponseDelegate == null)
        return;
      try
      {
        this.handleResponseDelegate(this.responseString);
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) ex);
      }
    }
  }
}
