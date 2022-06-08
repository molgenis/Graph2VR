using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMenuSliderNob : MonoBehaviour
{
  GameObject controler;
  CircleMenu menu;

  public void Set(CircleMenu menu)
  {
    this.menu = menu;
    controler = GameObject.FindGameObjectWithTag("RightController");
  }

  // Update is called once per frame
  bool lastGrip = true;
  void Update()
  {
    // Collider ray check
    RaycastHit hit;
    Collider collider = GetComponent<Collider>();
    bool actionPressed = false;
    if (ControlerInput.instance.gripRight && ControlerInput.instance.gripRight != lastGrip)
    {
      actionPressed = true;
    }

    if (ControlerInput.instance.gripRight)
    {
      // Calculate slider value
      Vector3 menuToControlerNormal = (controler.transform.position - menu.transform.position).normalized;
      Vector3 projected = Vector3.ProjectOnPlane(menuToControlerNormal, menu.transform.forward);
      menu.sliderValue = Mathf.Clamp01(Vector3.Angle(menu.transform.up, projected) / 180f);
    }
    else
    {
      if (collider != null)
      {
        bool pointerSelection = collider.Raycast(new Ray(controler.transform.position, controler.transform.forward), out hit, 2f);
        bool grabSelection = collider.Raycast(new Ray(controler.transform.position, transform.position - controler.transform.position), out hit, 0.1f);
        if (pointerSelection || grabSelection)
        {
          // Someone is pointing at us
          gameObject.GetComponent<Renderer>().material.color = menu.defaultColor + new Color(0.2f, 0.2f, 0.2f); ;

          // Someone is clicking at us
          if (actionPressed)
          {
            controler.transform.Find("Model").gameObject.SetActive(false);
            controler.transform.Find("Pointer").gameObject.SetActive(false);
          }
        }
        else
        {
          gameObject.GetComponent<Renderer>().material.color = menu.defaultColor;
        }
      }
    }
    lastGrip = ControlerInput.instance.gripRight;
  }
}
