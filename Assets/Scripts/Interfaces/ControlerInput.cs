using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlerInput : MonoBehaviour
{
   public bool gripLeft = false;
   public bool gripRight = false;
   public bool triggerLeft = false;
   public bool triggerRight = false;
   public bool menu = false;

   public bool viveLeftTrackpadClicked = false;
   public bool viveRightTrackpadClicked = false;

   public Vector2 axisLeft = Vector2.zero;
   public Vector2 axisRight = Vector2.zero;

   public InputActionProperty gripActionLeft;
   public InputActionProperty gripActionRight;

   public InputActionProperty triggerActionLeft;
   public InputActionProperty triggerActionRight;

   public InputActionProperty leftAxis;
   public InputActionProperty rightAxis;

   public InputActionProperty viveTrackpadLeftClick;
   public InputActionProperty viveTrackpadRightClick;

   public InputActionProperty viveTrackpadLeftTouchReleased;
   public InputActionProperty viveTrackpadRightTouchReleased;

   public InputActionProperty menuButton;



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
      if (leftAxis.action != null) leftAxis.action.canceled += LeftAxisCancel;
      if (rightAxis.action != null) rightAxis.action.Enable();
      if (rightAxis.action != null) rightAxis.action.performed += RightAxis;
      if (rightAxis.action != null) rightAxis.action.canceled += RightAxisCancel;

      if (viveTrackpadLeftClick.action != null) viveTrackpadLeftClick.action.Enable();
      if (viveTrackpadLeftClick.action != null) viveTrackpadLeftClick.action.performed += ViveTrackpadLeftClick;
      if (viveTrackpadRightClick.action != null) viveTrackpadRightClick.action.Enable();
      if (viveTrackpadRightClick.action != null) viveTrackpadRightClick.action.performed += ViveTrackpadRightClick;

      if (viveTrackpadLeftTouchReleased.action != null) viveTrackpadLeftTouchReleased.action.Enable();
      if (viveTrackpadLeftTouchReleased.action != null) viveTrackpadLeftTouchReleased.action.performed += ViveTrackpadLeftTouchReleased;
      if (viveTrackpadRightTouchReleased.action != null) viveTrackpadRightTouchReleased.action.Enable();
      if (viveTrackpadRightTouchReleased.action != null) viveTrackpadRightTouchReleased.action.performed += ViveTrackpadRightTouchReleased;

      if (menuButton.action != null) menuButton.action.Enable();
      if (menuButton.action != null) menuButton.action.performed += MenuButton;
      if (menuButton.action != null) menuButton.action.canceled += MenuButton;
   }

   void OnDisable()
   {
      if (gripActionLeft.action != null) gripActionLeft.action.performed -= GripActionLeft;
      if (gripActionRight.action != null) gripActionRight.action.performed -= GripActionRight;
      if (triggerActionLeft.action != null) gripActionLeft.action.performed -= TriggerActionLeft;
      if (triggerActionRight.action != null) gripActionRight.action.performed -= TriggerActionRight;
      if (leftAxis.action != null) leftAxis.action.performed -= LeftAxis;
      if (rightAxis.action != null) rightAxis.action.performed -= RightAxis;
      if (menuButton.action != null) menuButton.action.performed -= MenuButton;
   }

   void LeftAxis(InputAction.CallbackContext a)
   {
      axisLeft = (Vector2)a.ReadValueAsObject();
   }
   void LeftAxisCancel(InputAction.CallbackContext a)
   {
      axisLeft = Vector2.zero;
   }

   void RightAxis(InputAction.CallbackContext a)
   {
      axisRight = (Vector2)a.ReadValueAsObject();
   }
   void RightAxisCancel(InputAction.CallbackContext a)
   {
      axisRight = Vector2.zero;
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

   void ViveTrackpadLeftClick(InputAction.CallbackContext a)
   {
      viveLeftTrackpadClicked = a.ReadValueAsButton();
   }

   void ViveTrackpadRightClick(InputAction.CallbackContext a)
   {
      viveRightTrackpadClicked = a.ReadValueAsButton();
   }

   void ViveTrackpadLeftTouchReleased(InputAction.CallbackContext a)
   {
      axisLeft = Vector2.zero;
   }

   void ViveTrackpadRightTouchReleased(InputAction.CallbackContext a)
   {
      axisRight = Vector2.zero;
   }
   void MenuButton(InputAction.CallbackContext a)
   {
      menu = a.ReadValueAsButton();
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
