using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CircleMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
  CircleMenu menu;
  float initialAngle;
  readonly float falloff = 30f;
  GameObject controler;
  CircleMenu.CircleButton button;

  public void Set(CircleMenu menu, float initialAngle, CircleMenu.CircleButton button)
  {
    this.menu = menu;
    this.initialAngle = initialAngle;
    this.button = button;

    controler = GameObject.FindGameObjectWithTag("RightControler");
  }

  void Update()
  {
    // Auto rotate based on slider
    float angle = menu.GetMenuAngle() + initialAngle;
    transform.localPosition = new Vector2(Mathf.Sin(-angle * Mathf.Deg2Rad), Mathf.Cos(-angle * Mathf.Deg2Rad)); ;
    transform.localRotation = Quaternion.Euler(0, 0, angle);

    // Auto scale down if angle is outside of 0 ... 180
    float scaleFactor = 0;
    if (angle < 0)
    {
      scaleFactor = 1 - (Mathf.Clamp(-angle, 0, falloff) / falloff);
    }
    if (angle >= 0 && angle <= 180)
    {
      scaleFactor = 1;
    }
    if (angle > 180)
    {
      scaleFactor = 1 - (Mathf.Clamp(angle - 180, 0, falloff) / falloff);
    }
    transform.localScale = Vector3.one * scaleFactor;
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    gameObject.GetComponent<Renderer>().material.color = button.color + new Color(0.2f, 0.2f, 0.2f); ;
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    gameObject.GetComponent<Renderer>().material.color = button.color;
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    button.callback();
  }
}
