using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlerInput : MonoBehaviour
{
  public bool gripLeft = false;
  public bool gripRight = false;
  public bool triggerLeft = false;
  public bool triggerRight = false;
  public Vector2 axisLeft = Vector2.zero;
  public Vector2 axisRight = Vector2.zero;

  public InputActionProperty gripActionLeft;
  public InputActionProperty gripActionRight;

  public InputActionProperty triggerActionLeft;
  public InputActionProperty triggerActionRight;

  public InputActionProperty leftAxis;
  public InputActionProperty rightAxis;

  public InputActionProperty viveTouchpadLeftClick;
  public InputActionProperty viveTouchpadRightClick;

  public InputActionProperty viveTouchpadLeftTouchReleased;
  public InputActionProperty viveTouchpadRightTouchReleased;


  void OnEnable()
  {
    if (gripActionLeft.action != null) gripActionLeft.action.Enable();
    if (gripActionLeft.action != null) gripActionLeft.action.performed += GripActionLeft;
    if (gripActionRight.action != null) gripActionRight.action.Enable();
    if (gripActionRight.action != null) gripActionRight.action.performed += GripActionRight;

    if (triggerActionLeft.action != null) triggerActionLeft.action.Enable();
    if (triggerActionLeft.action != null) triggerActionLeft.action.performed += TriggerActionLeft;
    if (triggerActionRight.action != null) triggerActionRight.action.Enable();
    if (triggerActionRight.action != null) triggerActionRight.action.performed += TriggerActionRight;

    if (leftAxis.action != null) leftAxis.action.Enable();
    if (leftAxis.action != null) leftAxis.action.performed += LeftAxis;
    if (rightAxis.action != null) rightAxis.action.Enable();
    if (rightAxis.action != null) rightAxis.action.performed += RightAxis;

    if (viveTouchpadLeftClick.action != null) viveTouchpadLeftClick.action.Enable();
    if (viveTouchpadLeftClick.action != null) viveTouchpadLeftClick.action.performed += ViveTouchpadLeftClick;
    if (viveTouchpadRightClick.action != null) viveTouchpadRightClick.action.Enable();
    if (viveTouchpadRightClick.action != null) viveTouchpadRightClick.action.performed += ViveTouchpadRightClick;

    if (viveTouchpadLeftTouchReleased.action != null) viveTouchpadLeftTouchReleased.action.Enable();
    if (viveTouchpadLeftTouchReleased.action != null) viveTouchpadLeftTouchReleased.action.performed += ViveTouchpadLeftTouchReleased;
    if (viveTouchpadRightTouchReleased.action != null) viveTouchpadRightTouchReleased.action.Enable();
    if (viveTouchpadRightTouchReleased.action != null) viveTouchpadRightTouchReleased.action.performed += ViveTouchpadRightTouchReleased;
  }

  void OnDisable()
  {
    if (gripActionLeft.action != null) gripActionLeft.action.performed -= GripActionLeft;
    if (gripActionRight.action != null) gripActionRight.action.performed -= GripActionRight;
    if (triggerActionLeft.action != null) gripActionLeft.action.performed -= TriggerActionLeft;
    if (triggerActionRight.action != null) gripActionRight.action.performed -= TriggerActionRight;
    if (leftAxis.action != null) leftAxis.action.performed -= LeftAxis;
    if (rightAxis.action != null) rightAxis.action.performed -= RightAxis;

  }

  void LeftAxis(InputAction.CallbackContext a)
  {
    axisLeft = (Vector2)a.ReadValueAsObject();
  }
  void RightAxis(InputAction.CallbackContext a)
  {
    axisRight = (Vector2)a.ReadValueAsObject();
  }

  void GripActionLeft(InputAction.CallbackContext a)
  {
    gripLeft = a.ReadValueAsButton();
  }
  void GripActionRight(InputAction.CallbackContext a)
  {
    gripRight = a.ReadValueAsButton();
  }
  void TriggerActionLeft(InputAction.CallbackContext a)
  {
    triggerLeft = a.ReadValueAsButton();
  }
  void TriggerActionRight(InputAction.CallbackContext a)
  {
    triggerRight = a.ReadValueAsButton();
  }

  void ViveTouchpadLeftClick(InputAction.CallbackContext a)
  {
    Debug.Log("ViveTouchpadLeftClick");
  }

  void ViveTouchpadRightClick(InputAction.CallbackContext a)
  {
    Debug.Log("ViveTouchpadRightClick");
  }

  void ViveTouchpadLeftTouchReleased(InputAction.CallbackContext a)
  {
    Debug.Log("ViveTouchpadLeftTouchReleased");
    axisLeft = Vector2.zero;
  }

  void ViveTouchpadRightTouchReleased(InputAction.CallbackContext a)
  {
    Debug.Log("ViveTouchpadRightTouchReleased");
    axisRight = Vector2.zero;
  }

  public static ControlerInput instance;
  private void Awake()
  {
    instance = this;
  }

  public void VibrateLeft()
  {
    List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
    UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Left, devices);

    foreach (var device in devices)
    {
      UnityEngine.XR.HapticCapabilities capabilities;
      if (device.TryGetHapticCapabilities(out capabilities))
      {
        if (capabilities.supportsImpulse)
        {
          uint channel = 0;
          float amplitude = 0.5f;
          float duration = 0.5f;
          device.SendHapticImpulse(channel, amplitude, duration);
        }
      }
    }
  }

  public void VibrateRight()
  {
    List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
    UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Right, devices);

    foreach (var device in devices)
    {
      UnityEngine.XR.HapticCapabilities capabilities;
      if (device.TryGetHapticCapabilities(out capabilities))
      {
        if (capabilities.supportsImpulse)
        {
          uint channel = 0;
          float amplitude = 0.5f;
          float duration = 0.5f;
          device.SendHapticImpulse(channel, amplitude, duration);
        }
      }
    }
  }
}
