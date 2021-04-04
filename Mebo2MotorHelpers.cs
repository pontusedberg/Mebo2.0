// Decompiled with JetBrains decompiler
// Type: Mebo2MotorHelpers
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1844E21B-8F26-4849-82BD-78183271FBE6
// Assembly location: C:\Users\Gedyy\Desktop\tttt\assets\bin\Data\Managed\Assembly-CSharp.dll

using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class Mebo2MotorHelpers
{
  private const string COMMAND_START = "?";
  private const string COMMAND_CONJOIN = "&";
  public const int NO_PARAM = 65535;
  private const int TIME_JOINT = 800;
  private const int TIME_WHEEL = 1800;
  private static int iCmdIdx;

  private static char ToBase64(int val)
  {
    return "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"[val & 63];
  }

  private static string EncodeBase64(int val, int nBits)
  {
    StringBuilder stringBuilder = new StringBuilder(10);
    int num = 0;
    while (num < nBits)
    {
      stringBuilder.Append(Mebo2MotorHelpers.ToBase64(val >> num));
      num += 6;
    }
    return stringBuilder.ToString();
  }

  public static string NewCmd()
  {
    return "!" + (object) Mebo2MotorHelpers.ToBase64(Mebo2MotorHelpers.iCmdIdx++ & 63);
  }

  public static string EncodeSpeed(int iSpeed)
  {
    return Mebo2MotorHelpers.EncodeBase64(iSpeed, 12);
  }

  public static string GenerateMessage(params MeboCommand[] commands)
  {
    string str = "?";
    for (int index = 0; index < commands.Length; ++index)
    {
      str += Mebo2MotorHelpers.GenerateSingleCommand(index + 1, commands[index], (int) ushort.MaxValue);
      if (index != commands.Length - 1)
        str += "&";
    }
    return str;
  }

  public static string GenerateMessage(params Mebo2MotorHelpers.MotorRequest[] commands)
  {
    string str = "?";
    for (int index = 0; index < commands.Length; ++index)
    {
      str += Mebo2MotorHelpers.GenerateSingleCommand(index + 1, commands[index].Command, commands[index].Parameter);
      if (index != commands.Length - 1)
        str += "&";
    }
    return str;
  }

  public static string GenerateMessage(MeboCommand command, int parameter)
  {
    return "?" + Mebo2MotorHelpers.GenerateSingleCommand(1, command, parameter);
  }

  public static string GenerateMessage(MeboCommand command1, int parameter1, MeboCommand command2, int parameter2)
  {
    return "?" + Mebo2MotorHelpers.GenerateSingleCommand(1, command1, parameter1) + "&" + Mebo2MotorHelpers.GenerateSingleCommand(2, command2, parameter2);
  }

  public static string GenerateMessage(MeboCommand command1, int parameter1, MeboCommand command2, int parameter2, MeboCommand command3, int parameter3)
  {
    return "?" + Mebo2MotorHelpers.GenerateSingleCommand(1, command1, parameter1) + "&" + Mebo2MotorHelpers.GenerateSingleCommand(2, command2, parameter2) + "&" + Mebo2MotorHelpers.GenerateSingleCommand(3, command3, parameter3);
  }

  public static string GenerateMessage(MeboCommand command1, int parameter1, MeboCommand command2, int parameter2, MeboCommand command3, int parameter3, MeboCommand command4, int parameter4)
  {
    return "?" + Mebo2MotorHelpers.GenerateSingleCommand(1, command1, parameter1) + "&" + Mebo2MotorHelpers.GenerateSingleCommand(2, command2, parameter2) + "&" + Mebo2MotorHelpers.GenerateSingleCommand(3, command3, parameter3) + "&" + Mebo2MotorHelpers.GenerateSingleCommand(4, command4, parameter4);
  }

  private static string GenerateSingleCommand(int number, MeboCommand command, int parameter)
  {
    string str = Mebo2MotorHelpers.CommandString(command, parameter);
    if (string.IsNullOrEmpty(str))
    {
      Debug.LogError((object) ("ERROR GETTING COMMAND : " + (object) command));
      str = Mebo2MotorHelpers.CommandString(MeboCommand.none, (int) ushort.MaxValue);
    }
    return nameof (command) + (object) number + "=mebolink_message_send(" + str + ")";
  }

  public static string CommandString(MeboCommand cmd, int para)
  {
    switch (cmd)
    {
      case MeboCommand.READERS:
        return "READERS=?";
      case MeboCommand.FACTORY:
        return Mebo2MotorHelpers.NewCmd() + "P";
      case MeboCommand.BAT:
        return "BAT=?";
      case MeboCommand.WHEEL_LEFT_FORWARD:
        return Mebo2MotorHelpers.NewCmd() + "F" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.WHEEL_LEFT_BACKWARD:
        return Mebo2MotorHelpers.NewCmd() + "F" + Mebo2MotorHelpers.EncodeSpeed(-para);
      case MeboCommand.WHEEL_RIGHT_FORWARD:
        return Mebo2MotorHelpers.NewCmd() + "E" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.WHEEL_RIGHT_BACKWARD:
        return Mebo2MotorHelpers.NewCmd() + "E" + Mebo2MotorHelpers.EncodeSpeed(-para);
      case MeboCommand.WHEEL_BOTH_STOP:
        return Mebo2MotorHelpers.NewCmd() + "B";
      case MeboCommand.ARM_UP:
        return Mebo2MotorHelpers.NewCmd() + "G" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.ARM_DOWN:
        return Mebo2MotorHelpers.NewCmd() + "G" + Mebo2MotorHelpers.EncodeSpeed(-para);
      case MeboCommand.ARM_POSITION:
        return Mebo2MotorHelpers.NewCmd() + "K" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.ARM_STOP:
        return Mebo2MotorHelpers.NewCmd() + "CEAA";
      case MeboCommand.ARM_QUERY:
        return "ARM=?";
      case MeboCommand.WRIST_UD_UP:
        return Mebo2MotorHelpers.NewCmd() + "H" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.WRIST_UD_DOWN:
        return Mebo2MotorHelpers.NewCmd() + "H" + Mebo2MotorHelpers.EncodeSpeed(-para);
      case MeboCommand.WRIST_UD_POSITION:
        return Mebo2MotorHelpers.NewCmd() + "L" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.WRIST_UD_STOP:
        return Mebo2MotorHelpers.NewCmd() + "CIAA";
      case MeboCommand.WRIST_UD_QUERY:
        return "WRIST_UD=?";
      case MeboCommand.WRIST_ROTATE_LEFT:
        return Mebo2MotorHelpers.NewCmd() + "I" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.WRIST_ROTATE_RIGHT:
        return Mebo2MotorHelpers.NewCmd() + "I" + Mebo2MotorHelpers.EncodeSpeed(-para);
      case MeboCommand.WRIST_ROTATE_POSITION:
        return Mebo2MotorHelpers.NewCmd() + "M" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.WRIST_ROTATE_STOP:
        return Mebo2MotorHelpers.NewCmd() + "CQAA";
      case MeboCommand.WRIST_ROTATE_QUERY:
        return "WRIST_ROTATE=?";
      case MeboCommand.CLAW_POSITION:
        return Mebo2MotorHelpers.NewCmd() + "N" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.CLAW_STOP:
        return Mebo2MotorHelpers.NewCmd() + "CgAA";
      case MeboCommand.CLAW_QUERY:
        return "CLAW=?";
      case MeboCommand.CAL_ARM:
        return Mebo2MotorHelpers.NewCmd() + "DE";
      case MeboCommand.CAL_WRIST_UD:
        return Mebo2MotorHelpers.NewCmd() + "DI";
      case MeboCommand.CAL_WRIST_ROTATE:
        return Mebo2MotorHelpers.NewCmd() + "DQ";
      case MeboCommand.CAL_CLAW:
        return Mebo2MotorHelpers.NewCmd() + "Dg";
      case MeboCommand.VERSION_QUERY:
        return "VER=?";
      case MeboCommand.REBOOT_CMD:
        return Mebo2MotorHelpers.NewCmd() + "DE";
      case MeboCommand.JOINT_SPEED:
        return string.Empty;
      case MeboCommand.CAL_ALL:
        return Mebo2MotorHelpers.NewCmd() + "D_";
      case MeboCommand.SET_REG:
        return string.Empty;
      case MeboCommand.QUERY_REG:
        int num = para;
        return "REG" + (num / 100 % 10).ToString() + (num / 10 % 10).ToString() + (num % 10).ToString() + "=?";
      case MeboCommand.SAVE_REG:
        return "REG=FLUSH";
      case MeboCommand.WHEEL_LEFT_SPEED:
        return Mebo2MotorHelpers.NewCmd() + "F" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.WHEEL_RIGHT_SPEED:
        return Mebo2MotorHelpers.NewCmd() + "E" + Mebo2MotorHelpers.EncodeSpeed(para);
      case MeboCommand.QUERY_EVENT:
        return "*";
      default:
        Debug.LogError((object) ("UNSUPPORTED COMMAND : " + (object) cmd));
        return string.Empty;
    }
  }

  public static void SendRequest(MeboTags tag, string urlToRequest, Mebo2SettingsComms.HandleResponseDelegate responseHandler = null, int timeoutMilliseconds = 3000)
  {
    if (MeboSetupController.SKIPPED_CONNECTION)
      return;
    Mebo2SettingsComms.FlushQueue(tag);
    Mebo2SettingsComms.SendRequest(urlToRequest, responseHandler, timeoutMilliseconds, tag);
  }

  [StructLayout(LayoutKind.Sequential, Size = 1)]
  public struct MotorRequest
  {
    public MotorRequest(MeboCommand command, int parameter)
    {
      this.Command = command;
      this.Parameter = parameter;
    }

    public MeboCommand Command { get; private set; }

    public int Parameter { get; private set; }
  }

  public delegate void CallbackResponse(string response);
}
