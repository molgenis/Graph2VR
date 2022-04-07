using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ControllerType : MonoBehaviour
{
  private string type = "unknown";
  List<Action<string>> callbacks = new List<Action<string>>();
  public void GetControllerName(Action<string> callback)
  {
    this.callbacks.Add(callback);
    CallAllCallbacks();
  }

  void CallAllCallbacks()
  {
    if (type != "unknown")
    {
      foreach (Action<string> call in callbacks)
      {
        call(type);
      }
      callbacks.Clear();
      InputDevices.deviceConnected -= DeviceConnected;
    }
  }

  void Start()
  {
    InputDevices.deviceConnected += DeviceConnected;
    List<InputDevice> devices = new List<InputDevice>();
    InputDevices.GetDevices(devices);
    foreach (var device in devices)
      DeviceConnected(device);
  }

  void DeviceConnected(InputDevice device)
  {
    if ((device.characteristics & InputDeviceCharacteristics.Left) != 0)
    {
      Debug.Log("Controller: " + device.name);
      if (device.name == "Oculus Touch Controller OpenXR")
      {
        type = "quest";
      }
      if (device.name == "Vive")
      {
        type = "vive";
      }
      CallAllCallbacks();
    }
  }

  static public ControllerType instance;
  private void Awake()
  {
    instance = this;
  }
}
