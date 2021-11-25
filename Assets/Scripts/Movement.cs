using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Movement : MonoBehaviour
{
  private SteamVR_Action_Boolean snapTurnLeft = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnLeft");
  private SteamVR_Action_Boolean snapTurnRight = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnRight");
  private SteamVR_Action_Boolean teleport = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");
  private SteamVR_Action_Vector2 movement = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("Movement");

  public SteamVR_Input_Sources leftHand;
  public SteamVR_Input_Sources rightHand;
  public float snapTurnAngle = 20;
  public float movementSpeed = 10;

  private LineRenderer line = null;
  private Vector3 teleportPoint = Vector3.zero;
  public Transform teleportHand;
  void Start()
  {
    snapTurnLeft[rightHand].onChange += SteamVR_Behaviour_SnapTurnLeft_OnChange;
    snapTurnRight[rightHand].onChange += SteamVR_Behaviour_SnapTurnRight_OnChange;
    teleport[rightHand].onChange += SteamVR_Behaviour_Teleport_OnChange;
    movement[leftHand].onChange += SteamVR_Behaviour_Movement_OnChange;
    line = GetComponent<LineRenderer>();
    line.enabled = false;
  }

  private void SteamVR_Behaviour_Movement_OnChange(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
  {
    transform.position = transform.position + Camera.main.transform.rotation * (new Vector3(axis.x, 0, axis.y) * movementSpeed * Time.deltaTime);
  }

  private void SteamVR_Behaviour_Teleport_OnChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
  {
    if (newState) {
      line.enabled = true;
    } else {
      line.enabled = false;
      transform.position = teleportPoint;
    }
  }

  private void SteamVR_Behaviour_SnapTurnLeft_OnChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
  {
    if (newState) transform.Rotate(0, -snapTurnAngle, 0);
  }

  private void SteamVR_Behaviour_SnapTurnRight_OnChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
  {
    if (newState) transform.Rotate(0, snapTurnAngle, 0);
  }

  private void Update()
  {
    if (line.enabled) {
      Plane plane = new Plane(Vector3.up, Vector3.zero);
      Ray ray = new Ray(teleportHand.position, teleportHand.forward);
      teleportPoint = transform.position;
      line.SetPosition(0, Vector3.zero);
      line.SetPosition(1, Vector3.zero);
      if (plane.Raycast(ray, out float distance)) {
        if (distance < 50) {
          line.SetPosition(0, teleportHand.position);
          teleportPoint = teleportHand.position + (teleportHand.forward * distance);
          line.SetPosition(1, teleportPoint);
        }
      }
    }
  }
}
