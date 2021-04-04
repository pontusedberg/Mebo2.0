// Decompiled with JetBrains decompiler
// Type: Mebo2MotorController
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1844E21B-8F26-4849-82BD-78183271FBE6
// Assembly location: C:\Users\Gedyy\Desktop\tttt\assets\bin\Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

public class Mebo2MotorController : Singleton<Mebo2MotorController>, IMotorController
{
  private static int[] iJoint = new int[4];
  private static int[] iRegs = new int[1000];
  private bool g_Parsing;
  private bool bRebooting;
  private int iBattery;
  private int iVersion;
  private string m_TextStatusArm;
  private string m_TextStatusWristUpDown;
  private string m_TextStatusWristRotate;
  private string m_TextStatusClaw;

  public int StatusArmValue { get; private set; }

  public int StatusWristUpDownValue { get; private set; }

  public int StatusWristRotateValue { get; private set; }

  public int StatusClawValue { get; private set; }

  public bool StatusValuesAreValid { get; private set; }

  public bool StatusLEDValue { get; private set; }

  public void Initialize()
  {
    this.DoBat();
    for (int index = 0; index < Mebo2MotorController.iRegs.Length; ++index)
      Mebo2MotorController.iRegs[index] = -1;
    int num = 201;
    while (num < 213)
      ++num;
  }

  public void End()
  {
    this.Stop();
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
      Mebo2MotorController.iJoint[(int) joint] = result;
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
      if (joint != MeboJoint.CLAW)
        return;
      this.m_TextStatusClaw = result.ToString() + "%";
      this.StatusClawValue = result;
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
      Mebo2MotorController.iRegs[result2] = result1;
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
    int num12 = 0;
    foreach (Touch touch in Input.touches)
    {
      if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
        ++num12;
    }
    if (num12 != 0 || Input.GetMouseButton(0))
      ;
    this.m_TextStatusClaw = num11.ToString() + "%";
    this.StatusClawValue = (int) num11;
    this.StatusValuesAreValid = true;
    this.g_Parsing = false;
  }

  public void DoWheelLeftForward()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.LEFT_MOTOR, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.WHEEL_LEFT_FORWARD
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelLeftForwardCallback))), 3000);
  }

  public void DoWheelLeftForward(int speed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.LEFT_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_FORWARD, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelLeftForwardCallback))), 3000);
  }

  public void DoWheelLeftBackward()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.LEFT_MOTOR, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.WHEEL_LEFT_BACKWARD
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelLeftBackwardCallback))), 3000);
  }

  public void DoWheelLeftBackward(int speed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.LEFT_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_BACKWARD, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelLeftBackwardCallback))), 3000);
  }

  public void DoWheelRightForward()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.RIGHT_MOTOR, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.WHEEL_RIGHT_FORWARD
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelRightForwardCallback))), 3000);
  }

  public void DoWheelRightForward(int speed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.RIGHT_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_RIGHT_FORWARD, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelRightForwardCallback))), 3000);
  }

  public void DoWheelRightBackward()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.RIGHT_MOTOR, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.WHEEL_RIGHT_BACKWARD
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelRightBackwardCallback))), 3000);
  }

  public void DoWheelRightBackward(int speed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.RIGHT_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_RIGHT_BACKWARD, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelRightBackwardCallback))), 3000);
  }

  public void DoWheelBothForward()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_FORWARD, (int) ushort.MaxValue, MeboCommand.WHEEL_RIGHT_FORWARD, (int) ushort.MaxValue), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelBothForwardCallback))), 3000);
  }

  public void DoWheelBothForward(int speed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_FORWARD, speed, MeboCommand.WHEEL_RIGHT_FORWARD, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelBothForwardCallback))), 3000);
  }

  public void DoWheelBothForward(int leftSpeed, int rightSpeed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_FORWARD, leftSpeed, MeboCommand.WHEEL_RIGHT_FORWARD, rightSpeed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelBothForwardCallback))), 3000);
  }

  public void DoWheelBothBackward()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_BACKWARD, (int) ushort.MaxValue, MeboCommand.WHEEL_RIGHT_BACKWARD, (int) ushort.MaxValue), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelBothBackwardCallback))), 3000);
  }

  public void DoWheelBothBackward(int speed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_BACKWARD, speed, MeboCommand.WHEEL_RIGHT_BACKWARD, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelBothBackwardCallback))), 3000);
  }

  public void DoWheelBothBackward(int leftSpeed, int rightSpeed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_BACKWARD, leftSpeed, MeboCommand.WHEEL_RIGHT_BACKWARD, rightSpeed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, new Mebo2MotorHelpers.CallbackResponse(this.WheelBothBackwardCallback))), 3000);
  }

  public void DoWheelLeftStop()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.LEFT_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_FORWARD, 0), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWheelRightStop()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.RIGHT_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_RIGHT_FORWARD, 0), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWheelBothStop()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_FORWARD, 0, MeboCommand.WHEEL_RIGHT_FORWARD, 0), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWheelTurnLeft()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[2]
    {
      MeboCommand.WHEEL_LEFT_BACKWARD,
      MeboCommand.WHEEL_RIGHT_FORWARD
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWheelTurnLeft(int speed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_BACKWARD, speed, MeboCommand.WHEEL_RIGHT_FORWARD, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWheelTurnLeft(int leftSpeed, int rightSpeed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_BACKWARD, leftSpeed, MeboCommand.WHEEL_RIGHT_FORWARD, rightSpeed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWheelTurnRight()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[2]
    {
      MeboCommand.WHEEL_LEFT_FORWARD,
      MeboCommand.WHEEL_RIGHT_BACKWARD
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWheelTurnRight(int speed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_FORWARD, speed, MeboCommand.WHEEL_RIGHT_BACKWARD, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWheelTurnRight(int leftSpeed, int rightSpeed)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.BOTH_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WHEEL_LEFT_FORWARD, leftSpeed, MeboCommand.WHEEL_RIGHT_BACKWARD, rightSpeed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoArmPosition(int position)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.ARM_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.ARM_POSITION, position), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoArmUp(int speed = 100)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.ARM_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.ARM_UP, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoArmDown(int speed = 100)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.ARM_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.ARM_DOWN, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoArmStop()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.ARM_MOTOR, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.ARM_STOP
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoArmQuery()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.ARM_QUERY
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWristUdUp(int speed = 100)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.WRIST_UD_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WRIST_UD_UP, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWristUdDown(int speed = 100)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.WRIST_UD_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WRIST_UD_DOWN, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWristUDPosition(int position)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.WRIST_UD_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WRIST_UD_POSITION, position), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWristUdStop()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.WRIST_UD_MOTOR, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.WRIST_UD_STOP
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWristRotateLeft(int speed = 100)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.WRIST_LR_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WRIST_ROTATE_LEFT, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWristRotateRight(int speed = 100)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.WRIST_LR_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WRIST_ROTATE_RIGHT, speed), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWristRotatePosition(int position)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.WRIST_LR_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.WRIST_ROTATE_POSITION, position), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWristRotateStop()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.WRIST_LR_MOTOR, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.WRIST_ROTATE_STOP
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWristUdQuery()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.WRIST_UD_QUERY
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoWristRotateQuery()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.WRIST_ROTATE_QUERY
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoClawPosition(int position)
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.CLAW_MOTOR, Mebo2MotorHelpers.GenerateMessage(MeboCommand.CLAW_POSITION, position), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoClawStop()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.CLAW_MOTOR, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.CLAW_STOP
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoClawQuery()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.CLAW_QUERY
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoToggleLight()
  {
    this.StatusLEDValue = !this.StatusLEDValue;
    Mebo2SettingsComms.RequestClawLedToggle();
  }

  public void DoBat()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.BAT
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void DoJointSpeed()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.JOINT_SPEED
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  private void QueryPID(int p)
  {
  }

  public void DoQueryEvent()
  {
    Mebo2MotorHelpers.SendRequest(MeboTags.none, Mebo2MotorHelpers.GenerateMessage(new MeboCommand[1]
    {
      MeboCommand.QUERY_EVENT
    }), (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(r, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  private void BothSpeedCallback(string response)
  {
  }

  private void LeftSpeedCallback(string response)
  {
  }

  private void RightSpeedCallback(string response)
  {
  }

  private void LeftAndRightSpeedCallback(string response)
  {
  }

  private void WheelLeftForwardCallback(string response)
  {
  }

  private void WheelLeftBackwardCallback(string response)
  {
  }

  private void WheelRightForwardCallback(string response)
  {
  }

  private void WheelRightBackwardCallback(string response)
  {
  }

  private void WheelBothForwardCallback(string response)
  {
  }

  private void WheelBothBackwardCallback(string response)
  {
  }

  private void WheelLeftStopCallback(string response)
  {
  }

  private void WheelRightStopCallback(string response)
  {
  }

  private void WheelBothStopCallback(string response)
  {
  }

  private void ArmUpCallback(string response)
  {
  }

  private void ArmDownCallback(string response)
  {
  }

  private void ArmPositionCallback(string response)
  {
  }

  private void ArmStopCallback(string response)
  {
  }

  private void ArmQueryCallback(string response)
  {
  }

  private void WristUdUpCallback(string response)
  {
  }

  private void WristUdDownCallback(string response)
  {
  }

  private void WristUdPositionCallback(string response)
  {
  }

  private void WristUdStopCallback(string response)
  {
  }

  private void WristUdQueryCallback(string response)
  {
  }

  private void WristRotateLeftCallback(string response)
  {
  }

  private void WristRotateRightCallback(string response)
  {
  }

  private void WristRotatePositionCallback(string response)
  {
  }

  private void WristRotateStopCallback(string response)
  {
  }

  private void WristRotateQueryCallback(string response)
  {
  }

  private void ClawPositionCallback(string response)
  {
  }

  private void ClawStopCallback(string response)
  {
  }

  private void ClawQueryCallback(string response)
  {
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

  private void HandleCallback(string response, Mebo2MotorHelpers.CallbackResponse callback = null)
  {
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
  }

  public void Stop()
  {
    string command = Mebo2MotorHelpers.GenerateMessage(MeboCommand.ARM_STOP, MeboCommand.CLAW_STOP, MeboCommand.WHEEL_BOTH_STOP, MeboCommand.WRIST_ROTATE_STOP, MeboCommand.WRIST_UD_STOP);
    Mebo2MotorHelpers.SendRequest(MeboTags.none, command, (Mebo2SettingsComms.HandleResponseDelegate) (r => this.HandleCallback(command, (Mebo2MotorHelpers.CallbackResponse) null)), 3000);
  }

  public void Update()
  {
    throw new NotImplementedException();
  }
}
