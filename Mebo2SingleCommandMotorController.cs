// Decompiled with JetBrains decompiler
// Type: Mebo2SingleCommandMotorController
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1844E21B-8F26-4849-82BD-78183271FBE6
// Assembly location: C:\Users\Gedyy\Desktop\tttt\assets\bin\Data\Managed\Assembly-CSharp.dll

using System;
using System.Text;
using UnityEngine;

public class Mebo2SingleCommandMotorController : Singleton<Mebo2SingleCommandMotorController>, IMotorController
{
  private Mebo2SingleCommandMotorController.SingleCommandStatus m_CommandStatus = Mebo2SingleCommandMotorController.SingleCommandStatus.none;
  private int[] iJoint = new int[4];
  private int[] iRegs = new int[1000];
  private const string MeboPrefix1 = "http://192.168.99.1/ajax/command.json?command1=mebolink_message_send(";
  private const string MeboPrefix = "?command1=mebolink_message_send(";
  private const string MeboSuffix = ")";
  private const float SKIP_INIT_TIME = 2f;
  private float m_SkipInitTimer;
  private bool m_NeedToWaitForStop;
  public Mebo2SingleCommandMotorController.StatusDelegate OnInitialize;
  private string m_TextStatusArm;
  private string m_TextStatusWristUpDown;
  private string m_TextStatusWristRotate;
  private string m_TextStatusClaw;
  private int m_SetLeftTrackValue;
  private int m_SetRightTrackValue;
  public int m_SetArmValue;
  public float m_SetWristUDValue;
  public float m_SetWristLRValue;
  public float m_SetClawValue;
  private float m_SetClawValueVal;
  private float m_PreviousSetArmValue;
  private float m_PreviousSetWristUDValue;
  private float m_PreviousSetWristLRValue;
  private float m_PreviousClawValue;
  private bool m_HaveLightStatus;
  private float m_ArmDelta;
  private float m_WristUDDelta;
  private float m_WristLRDelta;
  private bool g_Parsing;
  private bool bRebooting;
  private int iBattery;
  private int iVersion;
  private int iCmdIdx;

  private bool IsInitialized
  {
    get
    {
      if (this.StatusValuesAreValid)
        return this.m_HaveLightStatus;
      return false;
    }
  }

  public int StatusArmValue { get; private set; }

  public int StatusClawValue { get; private set; }

  public bool StatusValuesAreValid { get; private set; }

  public int StatusWristRotateValue { get; private set; }

  public int StatusWristUpDownValue { get; private set; }

  public bool StatusLEDValue { get; private set; }

  public void DoWheelLeftForward()
  {
    this.m_SetLeftTrackValue = 100;
  }

  public void DoWheelLeftForward(int speed)
  {
    this.m_SetLeftTrackValue = speed;
  }

  public void DoWheelLeftBackward()
  {
    this.m_SetLeftTrackValue = -100;
  }

  public void DoWheelLeftBackward(int speed)
  {
    this.m_SetLeftTrackValue = -speed;
  }

  public void DoWheelRightForward()
  {
    this.m_SetRightTrackValue = 100;
  }

  public void DoWheelRightForward(int speed)
  {
    this.m_SetRightTrackValue = speed;
  }

  public void DoWheelRightBackward()
  {
    this.m_SetRightTrackValue = -100;
  }

  public void DoWheelRightBackward(int speed)
  {
    this.m_SetRightTrackValue = -speed;
  }

  public void DoWheelBothForward()
  {
    this.m_SetLeftTrackValue = 100;
    this.m_SetRightTrackValue = 100;
  }

  public void DoWheelBothForward(int speed)
  {
    this.m_SetLeftTrackValue = speed;
    this.m_SetRightTrackValue = speed;
  }

  public void DoWheelBothForward(int leftSpeed, int rightSpeed)
  {
    this.m_SetLeftTrackValue = leftSpeed;
    this.m_SetRightTrackValue = rightSpeed;
  }

  public void DoWheelBothBackward()
  {
    this.m_SetLeftTrackValue = -100;
    this.m_SetRightTrackValue = -100;
  }

  public void DoWheelBothBackward(int speed)
  {
    this.m_SetLeftTrackValue = -speed;
    this.m_SetRightTrackValue = -speed;
  }

  public void DoWheelBothBackward(int leftSpeed, int rightSpeed)
  {
    this.m_SetLeftTrackValue = -leftSpeed;
    this.m_SetRightTrackValue = -rightSpeed;
  }

  public void DoWheelLeftStop()
  {
    this.m_SetLeftTrackValue = 0;
  }

  public void DoWheelRightStop()
  {
    this.m_SetRightTrackValue = 0;
  }

  public void DoWheelBothStop()
  {
    this.m_SetLeftTrackValue = 0;
    this.m_SetRightTrackValue = 0;
  }

  public void DoWheelTurnLeft()
  {
    this.m_SetLeftTrackValue = -100;
    this.m_SetRightTrackValue = 100;
  }

  public void DoWheelTurnLeft(int speed)
  {
    this.m_SetLeftTrackValue = -speed;
    this.m_SetRightTrackValue = speed;
  }

  public void DoWheelTurnLeft(int leftSpeed, int rightSpeed)
  {
    this.m_SetLeftTrackValue = -leftSpeed;
    this.m_SetRightTrackValue = rightSpeed;
  }

  public void DoWheelTurnRight()
  {
    this.m_SetLeftTrackValue = 100;
    this.m_SetRightTrackValue = -100;
  }

  public void DoWheelTurnRight(int speed)
  {
    this.m_SetLeftTrackValue = speed;
    this.m_SetRightTrackValue = -speed;
  }

  public void DoWheelTurnRight(int leftSpeed, int rightSpeed)
  {
    this.m_SetLeftTrackValue = leftSpeed;
    this.m_SetRightTrackValue = -rightSpeed;
  }

  public void DoArmPosition(int position)
  {
    this.m_SetArmValue = position;
  }

  public void DoArmUp(int speed)
  {
    this.m_ArmDelta = 0.01f * (float) speed;
  }

  public void DoArmDown(int speed)
  {
    this.m_ArmDelta = 0.01f * (float) -speed;
  }

  public void DoArmStop()
  {
    this.m_ArmDelta = 0.0f;
    this.m_SetArmValue = this.StatusArmValue;
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.ARM_STOP
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r =>
    {
      this.HandleStatusCheck(r);
      this.m_NeedToWaitForStop = false;
    }), 3000);
    this.m_NeedToWaitForStop = true;
  }

  public void DoArmQuery()
  {
    throw new NotImplementedException();
  }

  public void DoWristUdUp(int speed)
  {
    this.m_WristUDDelta = 0.01f * (float) -speed;
  }

  public void DoWristUdDown(int speed)
  {
    this.m_WristUDDelta = 0.01f * (float) -speed;
  }

  public void DoWristUDPosition(int position)
  {
    this.m_SetWristUDValue = (float) position;
  }

  public void DoWristUdStop()
  {
    this.m_WristUDDelta = 0.0f;
    this.m_SetWristUDValue = (float) this.StatusWristUpDownValue;
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.WRIST_UD_STOP
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r =>
    {
      this.HandleStatusCheck(r);
      this.m_NeedToWaitForStop = false;
    }), 3000);
    this.m_NeedToWaitForStop = true;
  }

  public void DoWristRotateLeft(int speed)
  {
    this.m_WristLRDelta = 0.01f * (float) speed;
  }

  public void DoWristRotateRight(int speed)
  {
    this.m_WristLRDelta = 0.01f * (float) -speed;
  }

  public void DoWristRotatePosition(int position)
  {
    this.m_SetWristLRValue = (float) position;
  }

  public void DoWristRotateStop()
  {
    this.m_WristLRDelta = 0.0f;
    this.m_SetWristLRValue = (float) this.StatusWristRotateValue;
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.WRIST_ROTATE_STOP
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r =>
    {
      this.HandleStatusCheck(r);
      this.m_NeedToWaitForStop = false;
    }), 3000);
    this.m_NeedToWaitForStop = true;
  }

  public void DoWristUdQuery()
  {
    throw new NotImplementedException();
  }

  public void DoWristRotateQuery()
  {
    throw new NotImplementedException();
  }

  public void DoClawPosition(int position)
  {
    this.m_SetClawValue = (float) position;
  }

  public void DoClawStop()
  {
    this.m_SetClawValue = (float) this.StatusClawValue;
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.CLAW_STOP
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r =>
    {
      this.HandleStatusCheck(r);
      this.m_NeedToWaitForStop = false;
    }), 3000);
    this.m_NeedToWaitForStop = true;
  }

  public void DoClawQuery()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.CLAW_QUERY
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleStatusCheck(r)), 3000);
  }

  public void DoToggleLight()
  {
    this.StatusLEDValue = !this.StatusLEDValue;
  }

  public void DoBat()
  {
    throw new NotImplementedException();
  }

  public void DoJointSpeed()
  {
    throw new NotImplementedException();
  }

  public void DoQueryEvent()
  {
  }

  public void Initialize()
  {
    this.m_CommandStatus = Mebo2SingleCommandMotorController.SingleCommandStatus.Starting;
    Mebo2SettingsComms.onGetClawLedStateComplete += new Action<bool, bool>(this.OnClawLEDrequest);
    Mebo2SettingsComms.RequestGetClawLedState();
  }

  public void Update()
  {
    switch (this.m_CommandStatus)
    {
      case Mebo2SingleCommandMotorController.SingleCommandStatus.Starting:
        this.m_CommandStatus = Mebo2SingleCommandMotorController.SingleCommandStatus.SentStatusCheck;
        Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
        {
          MeboCommand.QUERY_EVENT
        }), new Mebo2SettingsComms.HandleResponseDelegate(this.HandleStatusCheck), 3000);
        this.ClearAllCommand();
        break;
      case Mebo2SingleCommandMotorController.SingleCommandStatus.SentStatusCheck:
        this.m_SkipInitTimer += Time.deltaTime;
        if ((double) this.m_SkipInitTimer >= 2.0)
          this.m_CommandStatus = Mebo2SingleCommandMotorController.SingleCommandStatus.Updating;
        this.ClearAllCommand();
        break;
      case Mebo2SingleCommandMotorController.SingleCommandStatus.Updating:
        this.UpdateAllCommand();
        break;
      default:
        this.ClearAllCommand();
        break;
    }
  }

  private void ComponentUpdate(ref float setCurrentVal, ref float previousSetVal, float delta, MeboCommand command)
  {
    float num = setCurrentVal + delta;
    setCurrentVal = Mathf.Clamp(num, 0.0f, 100f);
    if ((double) setCurrentVal != (double) previousSetVal)
    {
      Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
      {
        command
      }), (Mebo2SettingsComms.HandleResponseDelegate) (r =>
      {
        this.HandleStatusCheck(r);
        this.m_NeedToWaitForStop = false;
      }), 3000);
      this.m_NeedToWaitForStop = true;
    }
    previousSetVal = setCurrentVal;
  }

  public void End()
  {
    this.m_CommandStatus = Mebo2SingleCommandMotorController.SingleCommandStatus.none;
    this.OnInitialize = (Mebo2SingleCommandMotorController.StatusDelegate) null;
    this.m_TextStatusArm = (string) null;
    this.m_TextStatusWristUpDown = (string) null;
    this.m_TextStatusWristRotate = (string) null;
    this.m_TextStatusClaw = (string) null;
    this.StatusArmValue = 0;
    this.StatusClawValue = 0;
    this.StatusValuesAreValid = false;
    this.StatusWristRotateValue = 0;
    this.StatusWristUpDownValue = 0;
    this.m_SetLeftTrackValue = 0;
    this.m_SetRightTrackValue = 0;
    this.m_SetArmValue = 0;
    this.m_SetWristUDValue = 0.0f;
    this.m_SetWristLRValue = 0.0f;
    this.m_SetClawValue = 0.0f;
    this.m_PreviousSetArmValue = 0.0f;
    this.m_PreviousSetWristUDValue = 0.0f;
    this.m_PreviousSetWristLRValue = 0.0f;
    this.StatusLEDValue = false;
    this.m_ArmDelta = 0.0f;
    this.m_WristUDDelta = 0.0f;
    this.m_WristLRDelta = 0.0f;
    this.g_Parsing = false;
    this.bRebooting = false;
    this.iBattery = 0;
    this.iVersion = 0;
    this.iJoint = new int[4];
    this.iRegs = new int[1000];
  }

  private void OnClawLEDrequest(bool didSucceed, bool ledIsOn)
  {
    Mebo2SettingsComms.onGetClawLedStateComplete -= new Action<bool, bool>(this.OnClawLEDrequest);
    if (!didSucceed)
      return;
    this.StatusLEDValue = ledIsOn;
    this.m_HaveLightStatus = true;
  }

  public void Stop()
  {
  }

  private void UpdateAllCommand()
  {
    Mebo2UdpComms.UpdateRCommand(this.NewCmd() + "R" + this.EncodeBase64(this.m_SetRightTrackValue & (int) byte.MaxValue | (this.m_SetLeftTrackValue & (int) byte.MaxValue) << 8 | (this.m_SetArmValue & (int) byte.MaxValue) << 16, 24) + this.EncodeBase64((int) this.m_SetWristUDValue & (int) byte.MaxValue | ((int) this.m_SetWristLRValue & (int) byte.MaxValue) << 8 | ((int) this.m_SetClawValue & (int) byte.MaxValue) << 16 | (((!this.StatusLEDValue ? 0 : 1) | 28) & 63) << 24, 30));
  }

  private void ClearAllCommand()
  {
    Mebo2UdpComms.UpdateRCommand((string) null);
  }

  private void HandleStatusCheck(string response)
  {
    if (this.m_CommandStatus != Mebo2SingleCommandMotorController.SingleCommandStatus.SentStatusCheck)
      return;
    this.m_CommandStatus = Mebo2SingleCommandMotorController.SingleCommandStatus.Updating;
    string str = response;
    char[] chArray = new char[1]{ '\n' };
    foreach (string json in str.Split(chArray))
    {
      JsonResponse jsonResponse = JsonUtility.FromJson<JsonResponse>(json);
      if (jsonResponse != null && jsonResponse.response != null && jsonResponse.response.Length > 0)
      {
        if (jsonResponse.response.StartsWith("*"))
          this.ParseNewEvent(jsonResponse.response);
        else
          this.ParseResponse(jsonResponse.response);
      }
    }
    if (this.OnInitialize == null || !this.IsInitialized)
      return;
    this.StatusArmValue = 0;
    this.StatusWristRotateValue = 0;
    this.StatusWristUpDownValue = 0;
    this.OnInitialize(this.StatusWristUpDownValue, this.StatusWristRotateValue, this.StatusArmValue, this.StatusClawValue, this.StatusLEDValue);
    this.m_SetArmValue = this.StatusArmValue;
    this.m_SetWristLRValue = (float) this.StatusWristRotateValue;
    this.m_SetWristUDValue = (float) this.StatusWristUpDownValue;
    this.m_SetClawValue = (float) this.StatusClawValue;
    this.OnInitialize = (Mebo2SingleCommandMotorController.StatusDelegate) null;
  }

  public string NewCmd()
  {
    return "!" + (object) this.ToBase64(this.iCmdIdx++ & 63);
  }

  private char ToBase64(int val)
  {
    return "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"[val & 63];
  }

  private string EncodeBase64(int val, int nBits)
  {
    StringBuilder stringBuilder = new StringBuilder(10);
    int num = 0;
    while (num < nBits)
    {
      stringBuilder.Append(this.ToBase64(val >> num));
      num += 6;
    }
    return stringBuilder.ToString();
  }

  private int FromBase64(char ch)
  {
    if (ch >= 'A' && ch <= 'Z')
      return (int) ch - 65;
    if (ch >= 'a' && ch <= 'z')
      return 26 + (int) ch - 97;
    if (ch >= '0' && ch <= '9')
      return 52 + (int) ch - 48;
    if (ch == '-')
      return 62;
    return ch == '_' ? 63 : 0;
  }

  private void ParseJoint(MeboJoint joint, string response)
  {
    int result = 0;
    if (response.StartsWith("OK"))
    {
      if (joint == MeboJoint.ARM)
        this.m_TextStatusArm = "OK";
      if (joint == MeboJoint.WRIST_UD)
        this.m_TextStatusWristUpDown = "OK";
      if (joint == MeboJoint.WRIST_ROTATE)
        this.m_TextStatusWristRotate = "OK";
      if (joint != MeboJoint.CLAW)
        return;
      this.m_TextStatusClaw = "OK";
    }
    else if (response.StartsWith("BUSY"))
    {
      if (joint == MeboJoint.ARM)
        this.m_TextStatusArm = "Busy";
      if (joint == MeboJoint.WRIST_UD)
        this.m_TextStatusWristUpDown = "Busy";
      if (joint == MeboJoint.WRIST_ROTATE)
        this.m_TextStatusWristRotate = "Busy";
      if (joint != MeboJoint.CLAW)
        return;
      this.m_TextStatusClaw = "Busy";
    }
    else if (int.TryParse(response, out result))
    {
      if (result < 0 || result > 65536)
        return;
      this.iJoint[(int) joint] = result;
      if (joint == MeboJoint.ARM)
      {
        this.m_TextStatusArm = result.ToString() + "%";
        this.StatusArmValue = result;
      }
      if (joint == MeboJoint.WRIST_UD)
      {
        this.m_TextStatusWristUpDown = result.ToString() + "%";
        this.StatusWristUpDownValue = result;
      }
      if (joint == MeboJoint.WRIST_ROTATE)
      {
        this.m_TextStatusWristRotate = result.ToString() + "%";
        this.StatusWristRotateValue = result;
      }
      if (joint == MeboJoint.CLAW)
      {
        this.m_TextStatusClaw = result.ToString() + "%";
        this.StatusClawValue = result;
      }
      this.StatusValuesAreValid = true;
    }
    else
    {
      if (joint == MeboJoint.ARM)
        this.m_TextStatusArm = "?";
      if (joint == MeboJoint.WRIST_UD)
        this.m_TextStatusWristUpDown = "?";
      if (joint == MeboJoint.WRIST_ROTATE)
        this.m_TextStatusWristRotate = "?";
      if (joint != MeboJoint.CLAW)
        return;
      this.m_TextStatusClaw = "?";
    }
  }

  private void ParseResponse(string response)
  {
    if (response == null || response.Length < 1)
      return;
    if (response.StartsWith("$"))
      this.ParseNewResponse(response);
    else if (response.StartsWith("BAT="))
    {
      int result = 0;
      if (!int.TryParse(response.Substring(4), out result))
        return;
      this.iBattery = result;
    }
    else if (response.StartsWith("VER="))
    {
      int result = 0;
      if (!int.TryParse(response.Substring(4), out result))
        return;
      this.iVersion = result;
    }
    else if (response.StartsWith("REG"))
    {
      int result1 = 0;
      int result2 = 0;
      if (!int.TryParse(response.Substring(3, 3), out result2) || !int.TryParse(response.Substring(7), out result1))
        return;
      this.iRegs[result2] = result1;
    }
    else if (response.StartsWith("RST=OK"))
      this.bRebooting = true;
    else if (response.StartsWith("ARM="))
      this.ParseJoint(MeboJoint.ARM, response.Substring(4));
    else if (response.StartsWith("WRIST_UD="))
      this.ParseJoint(MeboJoint.WRIST_UD, response.Substring(9));
    else if (response.StartsWith("WRIST_RO="))
    {
      this.ParseJoint(MeboJoint.WRIST_ROTATE, response.Substring(9));
    }
    else
    {
      if (!response.StartsWith("CLAW="))
        return;
      this.ParseJoint(MeboJoint.CLAW, response.Substring(5));
    }
  }

  private void ParseNewResponse(string response)
  {
    if (response == null || response.Length < 3 || response[0] != '$')
      return;
    this.FromBase64(response[1]);
    this.FromBase64(response[2]);
  }

  private void ParseNewEvent(string response)
  {
    if (response == null || response.Length < 12 || (response[0] != '*' || response[1] != 'B'))
      return;
    uint num1 = (uint) (this.FromBase64(response[2]) | this.FromBase64(response[3]) << 6 | this.FromBase64(response[4]) << 12 | this.FromBase64(response[5]) << 18);
    uint num2 = (uint) (this.FromBase64(response[6]) | this.FromBase64(response[7]) << 6 | this.FromBase64(response[8]) << 12 | this.FromBase64(response[9]) << 18);
    uint num3 = (uint) (this.FromBase64(response[10]) | this.FromBase64(response[11]) << 6);
    int num4 = (int) (num1 >> 0) & (int) byte.MaxValue;
    if ((num4 & 128) != 0)
    {
      int num5 = num4 - 256;
    }
    int num6 = (int) (num1 >> 8) & (int) byte.MaxValue;
    if ((num6 & 128) != 0)
    {
      int num7 = num6 - 256;
    }
    uint num8 = num1 >> 16 & (uint) byte.MaxValue;
    uint num9 = num2 >> 0 & (uint) byte.MaxValue;
    uint num10 = num2 >> 8 & (uint) byte.MaxValue;
    uint num11 = num2 >> 16 & (uint) byte.MaxValue;
    this.g_Parsing = true;
    this.iBattery = (int) num3;
    this.StatusArmValue = (int) num8;
    this.StatusClawValue = (int) num11;
    this.StatusWristUpDownValue = (int) num9;
    this.StatusWristRotateValue = (int) num10;
    BatteryLevelChecker.SetBatteryLevel((float) this.iBattery);
    int num12 = 0;
    foreach (Touch touch in Input.touches)
    {
      if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
        ++num12;
    }
    this.StatusValuesAreValid = true;
    this.g_Parsing = false;
  }

  public void SendCommand_U(int param1, int param2 = 0)
  {
    WWW www = new WWW("?command1=mebolink_message_send(" + this.NewCmd() + "U" + this.EncodeBase64(param1, 12) + this.EncodeBase64(param2, 18) + ")");
    do
      ;
    while (!www.isDone);
  }

  public void SendCommand_V(int[] values)
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.Append("http://192.168.99.1/ajax/command.json?command1=mebolink_message_send(");
    stringBuilder.Append(Singleton<Mebo2SingleCommandMotorController>.Instance.NewCmd());
    stringBuilder.Append("V");
    stringBuilder.Append(this.EncodeBase64(213, 12));
    stringBuilder.Append(this.EncodeBase64(18, 6));
    foreach (int val in values)
      stringBuilder.Append(this.EncodeBase64(val, 18));
    stringBuilder.Append(")");
    WWW www = new WWW(stringBuilder.ToString());
    do
      ;
    while (!www.isDone);
  }

  private enum MeboSpdFlags
  {
    LIGHT_ON = 1,
    RESERVED = 2,
    SPD_ARM = 4,
    SPD_WRIST_UD = 8,
    SPD_WRIST_ROTATE = 16, // 0x00000010
    SPD_CLAW = 32, // 0x00000020
    SPD_MASK = 60, // 0x0000003C
  }

  private enum SingleCommandStatus
  {
    none = -1,
    Starting = 0,
    SentStatusCheck = 1,
    Updating = 2,
  }

  public delegate void StatusDelegate(int wristUDValue, int wristLRValue, int armValue, int clawValue, bool ledStatus);
}
