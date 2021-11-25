using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CircleMenuSliderNob : MonoBehaviour
{
  private SteamVR_Action_Boolean pinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
  GameObject controler;
  CircleMenu menu;

  public void Set(CircleMenu menu)
  {
    this.menu = menu;
    controler = GameObject.FindGameObjectWithTag("RightControler");
    pinchAction[SteamVR_Input_Sources.RightHand].onChange += SteamVR_Behaviour_Pinch_OnChange;
  }

  bool actionPressed = false;
  private void SteamVR_Behaviour_Pinch_OnChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
  {
    actionPressed = newState;
    if (newState == false) {
      controler.transform.Find("Model").gameObject.SetActive(true);
      controler.transform.Find("Pointer").gameObject.SetActive(true);
      held = false;
    }
  }

  bool held = false;
  // Update is called once per frame
  void Update()
  {
    // Collider ray check
    RaycastHit hit;
    Collider collider = GetComponent<Collider>();
    if (held) {
      // Calculate slider value
      Vector3 menuToControlerNormal = (controler.transform.position - menu.transform.position).normalized;
      Vector3 projected = Vector3.ProjectOnPlane(menuToControlerNormal, menu.transform.forward);
      menu.sliderValue = Mathf.Clamp01(Vector3.Angle(menu.transform.up, projected) / 180f);
    } else {
      if (collider != null) {
        bool pointerSelection = collider.Raycast(new Ray(controler.transform.position, controler.transform.forward), out hit, 2f);
        bool grabSelection = collider.Raycast(new Ray(controler.transform.position, transform.position - controler.transform.position), out hit, 0.1f);
        if (pointerSelection || grabSelection) {
          // Someone is pointing at us
          gameObject.GetComponent<Renderer>().material.color = menu.defaultColor + new Color(0.2f, 0.2f, 0.2f); ;

          // Someone is clicking at us
          if (actionPressed) {
            controler.transform.Find("Model").gameObject.SetActive(false);
            controler.transform.Find("Pointer").gameObject.SetActive(false);
            held = true;
          }
        } else {
          gameObject.GetComponent<Renderer>().material.color = menu.defaultColor;
        }
      }
    }
  }
}
