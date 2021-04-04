// Decompiled with JetBrains decompiler
// Type: Mebo2UIMessenger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1844E21B-8F26-4849-82BD-78183271FBE6
// Assembly location: C:\Users\Gedyy\Desktop\tttt\assets\bin\Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

public class Mebo2UIMessenger : MonoBehaviour
{
  private readonly float SQRT2 = Mathf.Sqrt(2f);
  private IMotorController m_ControllerInstance = (IMotorController) Singleton<Mebo2SingleCommandMotorController>.Instance;
  private const float JOYSTICK_BOUNDARY = 0.25f;
  private const float MOVE_REPEAT_TIME = 1f;
  private const float ARM_REPEAT_TIME = 0.5f;
  private const float WRIST_REPEAT_TIME = 0.1f;
  private const float PINCH_REPEAT_TIME = 0.1f;
  private const float PINCH_CHECKUP_TIME = 1f;
  [Header("Component Controllers")]
  [SerializeField]
  private TalkbackController m_TalkBackController;
  [SerializeField]
  private LiveVideoView m_VideoFeed;
  [SerializeField]
  private MicSampleReader m_MicSampleReader;
  [Header("Controls")]
  [SerializeField]
  private UIControlElement m_DrivingUDJoystick;
  [SerializeField]
  private UIControlElement m_DrivingLRJoystick;
  [SerializeField]
  private UIControlElement m_ShoulderJoystick;
  [SerializeField]
  private UIControlElement m_ElbowJoystick;
  [SerializeField]
  private UIControlElement m_WristRotator;
  [SerializeField]
  private UIControlElement m_PinchControl;
  [SerializeField]
  private UIControlElement m_WristMultiControlJoystick;
  [SerializeField]
  private Button m_LightButton;
  [Header("Graphics")]
  [SerializeField]
  private Sprite m_LightButtonOn;
  [SerializeField]
  private Sprite m_LightButtonOff;
  private Image m_LightButtonImage;
  private float m_LastUpdate;
  private bool m_UsingJoystick;
  private int m_BothTrackSpeed;
  private float m_JoystickUDValue;
  private float m_JoystickLRValue;
  private float m_LeftTrackValue;
  private float m_RightTrackValue;
  private static int s_LeftTrackSpeed;
  private static int s_RightTrackSpeed;
  private static int s_BothTrackSpeed;
  private bool m_WasStopped;
  private float lastQueryTime;
  private bool pinchControlsAreReady;
  private MeboDoubleMotor m_WheelMotor;
  private float m_ArmJoystickVal;
  private MeboMotor m_ArmMotor;
  private float m_WristUDValue;
  private float m_WristLRValue;
  private MeboMotor m_WristUDMotor;
  private MeboMotor m_WristLRMotor;
  private float m_CurrentPinchAmount;
  private string m_DebugOutput;

  private void Awake()
  {
    this.m_WheelMotor = new MeboDoubleMotor((Action) (() => this.MotorCallback(Mebo2UIMessenger.MotorSide.LEFT, MotorMovement.FORWARD)), (Action) (() => this.MotorCallback(Mebo2UIMessenger.MotorSide.LEFT, MotorMovement.STOP)), (Action) (() => this.MotorCallback(Mebo2UIMessenger.MotorSide.LEFT, MotorMovement.BACKWARD)), (Action) (() => this.MotorCallback(Mebo2UIMessenger.MotorSide.RIGHT, MotorMovement.FORWARD)), (Action) (() => this.MotorCallback(Mebo2UIMessenger.MotorSide.RIGHT, MotorMovement.STOP)), (Action) (() => this.MotorCallback(Mebo2UIMessenger.MotorSide.RIGHT, MotorMovement.BACKWARD)), (Action) (() => this.MotorCallback(Mebo2UIMessenger.MotorSide.BOTH, MotorMovement.FORWARD)), (Action) (() => this.MotorCallback(Mebo2UIMessenger.MotorSide.BOTH, MotorMovement.STOP)), (Action) (() => this.MotorCallback(Mebo2UIMessenger.MotorSide.BOTH, MotorMovement.BACKWARD)), (Action) (() => this.TurnCallback(Mebo2UIMessenger.MotorSide.LEFT)), (Action) (() => this.TurnCallback(Mebo2UIMessenger.MotorSide.RIGHT)));
    this.m_ArmMotor = new MeboMotor((Action) (() => this.m_ControllerInstance.DoArmUp(10)), (Action) (() => this.m_ControllerInstance.DoArmStop()), (Action) (() => this.m_ControllerInstance.DoArmDown(10)));
    this.m_WristUDMotor = new MeboMotor((Action) (() => this.m_ControllerInstance.DoWristUDPosition(100)), (Action) (() => this.m_ControllerInstance.DoWristUdStop()), (Action) (() => this.m_ControllerInstance.DoWristUDPosition(0)));
    this.m_WristLRMotor = new MeboMotor((Action) (() => this.m_ControllerInstance.DoWristRotatePosition(100)), (Action) (() => this.m_ControllerInstance.DoWristRotateStop()), (Action) (() => this.m_ControllerInstance.DoWristRotatePosition(0)));
    this.m_LightButtonImage = this.m_LightButton.GetComponent<Image>();
  }

  private void Start()
  {
    Mebo2SettingsComms.RequestGetClawLedState();
    Singleton<Mebo2SingleCommandMotorController>.Instance.OnInitialize = new Mebo2SingleCommandMotorController.StatusDelegate(this.OnCommunicationInitialized);
    this.m_ControllerInstance.Initialize();
  }

  private void OnCommunicationInitialized(int wristUDValue, int wristLRValue, int armValue, int clawValue, bool clawLED)
  {
    float num1 = Mathf.InverseLerp(0.0f, 100f, (float) wristUDValue);
    float num2 = Mathf.InverseLerp(0.0f, 100f, (float) wristLRValue);
    float num3 = Mathf.InverseLerp(0.0f, 100f, (float) armValue);
    float num4 = Mathf.InverseLerp(0.0f, 100f, (float) clawValue);
    Debug.Log((object) ("Initialized : " + (object) wristUDValue + "(" + (object) num1 + ")| " + (object) wristLRValue + "(" + (object) num2 + ")|" + (object) armValue + "(" + (object) num3 + ")|" + (object) clawValue + "(" + (object) num4 + ")"));
    this.m_WristRotator.SetValue(num2);
    this.m_ShoulderJoystick.SetValue(num3);
    this.m_WristMultiControlJoystick.SetValue(num1);
    this.m_PinchControl.SetValue(num4);
  }

  private void OnEnable()
  {
  }

  private void OnDisable()
  {
  }

  private void Update()
  {
    this.m_ControllerInstance.Update();
  }

  private void LateUpdate()
  {
    if ((double) this.lastQueryTime + 0.100000001490116 < (double) Time.time)
    {
      this.lastQueryTime = Time.time;
      this.m_ControllerInstance.DoQueryEvent();
    }
    this.DriveUpdate();
    this.ArmUpdate();
    this.WristUpdate();
  }

  private void OnDestroy()
  {
    this.m_ControllerInstance.End();
  }

  private int GetPower(float mag)
  {
    return Mathf.RoundToInt(Mathf.Lerp(10f, 100f, Mathf.InverseLerp(0.25f, 1f, mag)));
  }

  private void GetTrackValues(float x, float y, out float leftTrack, out float rightTrack)
  {
    this.GetTrackValues(new Vector2(x, y), out leftTrack, out rightTrack);
  }

  private void GetTrackValues(Vector2 v, out float leftTrack, out float rightTrack)
  {
    float magnitude = v.magnitude;
    float num1 = Mathf.Sin(-0.7853982f);
    float num2 = Mathf.Cos(-0.7853982f);
    float x = v.x;
    float y = v.y;
    Vector2 vector2 = new Vector2((float) ((double) num2 * (double) x - (double) num1 * (double) y), (float) ((double) num1 * (double) x + (double) num2 * (double) y)) * this.SQRT2;
    vector2.x = Mathf.Clamp(vector2.x, -1f, 1f);
    vector2.y = Mathf.Clamp(vector2.y, -1f, 1f);
    leftTrack = vector2.x;
    rightTrack = vector2.y;
  }

  private void HandleDriveJoystickMovement(UIControlElement joystick)
  {
    this.m_UsingJoystick = true;
    switch (joystick.InputType)
    {
      case InputTypes.Absoloute:
        this.HandleAbsolouteJoystick(joystick);
        break;
      case InputTypes.Trigger:
        this.HandleTriggerJoystick(joystick);
        break;
    }
  }

  private void HandleAbsolouteJoystick(UIControlElement joystick)
  {
    int position = Mathf.FloorToInt(Mathf.Lerp(0.0f, 100f, joystick.Value));
    switch (joystick.Type)
    {
      case JoystickType.Arm:
        this.m_ControllerInstance.DoArmPosition(position);
        break;
      case JoystickType.WristUD:
        this.m_ControllerInstance.DoWristUDPosition(position);
        break;
      case JoystickType.WristLR:
        this.m_ControllerInstance.DoWristRotatePosition(position);
        break;
    }
  }

  private void HandleTriggerJoystick(UIControlElement joystick)
  {
    float num1 = Mathf.Sin(joystick.Angle * ((float) Math.PI / 180f));
    float num2 = Mathf.Cos(joystick.Angle * ((float) Math.PI / 180f));
    float f1 = num1 * joystick.Value;
    float f2 = num2 * joystick.Value;
    if ((double) Mathf.Abs(f1) < 0.25)
      f1 = 0.0f;
    if ((double) Mathf.Abs(f2) < 0.25)
      f2 = 0.0f;
    switch (joystick.Type)
    {
      case JoystickType.DriveUD:
        this.m_JoystickUDValue = f2;
        break;
      case JoystickType.DriveLR:
        this.m_JoystickLRValue = f1;
        break;
      case JoystickType.Wrist:
        this.m_WristLRValue = f1;
        this.m_WristUDValue = f2;
        break;
      case JoystickType.Arm:
        this.m_ArmJoystickVal = f2;
        break;
      case JoystickType.WristUD:
        this.m_WristUDValue = f2;
        break;
      case JoystickType.WristLR:
        this.m_WristLRValue = f1;
        break;
    }
  }

  private void HandleDriveJoystickUp(UIControlElement joystick)
  {
    this.m_UsingJoystick = false;
    if (joystick.InputType == InputTypes.Absoloute)
      return;
    switch (joystick.Type)
    {
      case JoystickType.Drive:
        this.m_JoystickUDValue = 0.0f;
        this.m_JoystickLRValue = 0.0f;
        break;
      case JoystickType.DriveUD:
        this.m_JoystickUDValue = 0.0f;
        break;
      case JoystickType.DriveLR:
        this.m_JoystickLRValue = 0.0f;
        break;
      case JoystickType.Wrist:
        this.m_ControllerInstance.DoWristRotateStop();
        this.m_ControllerInstance.DoWristUdStop();
        break;
      case JoystickType.Arm:
        this.m_ControllerInstance.DoArmStop();
        break;
      case JoystickType.WristUD:
        this.m_ControllerInstance.DoWristUdStop();
        break;
      case JoystickType.WristLR:
        this.m_ControllerInstance.DoWristRotateStop();
        break;
      default:
        this.m_ControllerInstance.DoArmStop();
        this.m_ControllerInstance.DoClawStop();
        this.m_ControllerInstance.DoWheelBothStop();
        this.m_ControllerInstance.DoWristRotateStop();
        this.m_ControllerInstance.DoWristUdStop();
        break;
    }
  }

  private void LightButtonToggle()
  {
    this.m_ControllerInstance.DoToggleLight();
  }

  private void HandlePinchControl(UIControlElement pinch)
  {
    float num = pinch.Value;
    Debug.Log((object) ("======>>>>>>>>Pinch Value " + (object) num));
    this.m_ControllerInstance.DoClawPosition(Mathf.FloorToInt(num * 100f));
  }

  private void HandlePinchUp(UIControlElement pinch)
  {
    this.m_CurrentPinchAmount = pinch.Value;
  }

  private void DriveUpdate()
  {
    float leftTrack = 0.0f;
    float rightTrack = 0.0f;
    this.GetTrackValues(this.m_JoystickLRValue, this.m_JoystickUDValue, out leftTrack, out rightTrack);
    MeboMath.DifferentSignIgnoreZero(leftTrack, this.m_LeftTrackValue);
    MeboMath.DifferentSignIgnoreZero(rightTrack, this.m_RightTrackValue);
    int power1 = this.GetPower(Mathf.Abs(leftTrack));
    int power2 = this.GetPower(Mathf.Abs(rightTrack));
    bool flag = Mebo2UIMessenger.s_LeftTrackSpeed != power1 || Mebo2UIMessenger.s_RightTrackSpeed != power2;
    this.m_WheelMotor.Update(leftTrack, rightTrack);
    Mebo2UIMessenger.s_LeftTrackSpeed = power1;
    Mebo2UIMessenger.s_RightTrackSpeed = power2;
    this.m_LeftTrackValue = leftTrack;
    this.m_RightTrackValue = rightTrack;
    if (!flag)
      return;
    this.m_WheelMotor.Reset();
  }

  private void ArmUpdate()
  {
    this.m_ArmMotor.Update(this.m_ArmJoystickVal);
  }

  private void WristUpdate()
  {
    this.m_WristLRMotor.Update(this.m_WristLRValue);
    this.m_WristUDMotor.Update(this.m_WristUDValue);
  }

  private void MotorCallback(Mebo2UIMessenger.MotorSide motor, MotorMovement movement)
  {
    switch (motor)
    {
      case Mebo2UIMessenger.MotorSide.LEFT:
        switch (movement)
        {
          case MotorMovement.FORWARD:
            this.m_ControllerInstance.DoWheelLeftForward(Mebo2UIMessenger.s_LeftTrackSpeed);
            return;
          case MotorMovement.STOP:
            this.m_ControllerInstance.DoWheelLeftStop();
            return;
          case MotorMovement.BACKWARD:
            this.m_ControllerInstance.DoWheelLeftBackward(Mebo2UIMessenger.s_LeftTrackSpeed);
            return;
          default:
            return;
        }
      case Mebo2UIMessenger.MotorSide.RIGHT:
        switch (movement)
        {
          case MotorMovement.FORWARD:
            this.m_ControllerInstance.DoWheelRightForward(Mebo2UIMessenger.s_RightTrackSpeed);
            return;
          case MotorMovement.STOP:
            this.m_ControllerInstance.DoWheelRightStop();
            return;
          case MotorMovement.BACKWARD:
            this.m_ControllerInstance.DoWheelRightBackward(Mebo2UIMessenger.s_RightTrackSpeed);
            return;
          default:
            return;
        }
      case Mebo2UIMessenger.MotorSide.BOTH:
        switch (movement)
        {
          case MotorMovement.FORWARD:
            this.m_ControllerInstance.DoWheelBothForward(Mebo2UIMessenger.s_LeftTrackSpeed, Mebo2UIMessenger.s_RightTrackSpeed);
            return;
          case MotorMovement.STOP:
            this.m_ControllerInstance.DoWheelBothStop();
            return;
          case MotorMovement.BACKWARD:
            this.m_ControllerInstance.DoWheelBothBackward(Mebo2UIMessenger.s_LeftTrackSpeed, Mebo2UIMessenger.s_RightTrackSpeed);
            return;
          default:
            return;
        }
    }
  }

  private void TurnCallback(Mebo2UIMessenger.MotorSide motor)
  {
    if (motor != Mebo2UIMessenger.MotorSide.LEFT)
    {
      if (motor != Mebo2UIMessenger.MotorSide.RIGHT)
        return;
      this.m_ControllerInstance.DoWheelTurnRight(Mebo2UIMessenger.s_LeftTrackSpeed, Mebo2UIMessenger.s_RightTrackSpeed);
    }
    else
      this.m_ControllerInstance.DoWheelTurnLeft(Mebo2UIMessenger.s_LeftTrackSpeed, Mebo2UIMessenger.s_RightTrackSpeed);
  }

  private enum MotorSide
  {
    LEFT,
    RIGHT,
    BOTH,
    TURN,
  }
}
