using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CircleMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
  bool useAlternativeClick;
  CircleMenu menu;
  float initialAngle;
  readonly float falloff = 30f;
  GameObject controller;
  CircleMenu.CircleButton button;

  public void Set(CircleMenu menu, float initialAngle, CircleMenu.CircleButton button, bool useAlternativeClick = false)
  {
    this.menu = menu;
    this.initialAngle = initialAngle;
    this.button = button;
    this.useAlternativeClick = useAlternativeClick;

    controller = GameObject.FindGameObjectWithTag("RightController");
  }

  void Update()
  {
    if (!useAlternativeClick)
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
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    gameObject.GetComponent<Renderer>().material.color = button.color + new Color(0.2f, 0.2f, 0.2f);
    TextMeshPro text = gameObject.GetComponentInChildren<TextMeshPro>();
    if (text != null)
    {
      if (useAlternativeClick)
      {
        text.text = button.additionalLabel;
      }
      else
      {
        text.text = button.hoveredLabel;
        text.enableWordWrapping = true;
      }
    }
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    gameObject.GetComponent<Renderer>().material.color = button.color;
    TextMeshPro text = gameObject.GetComponentInChildren<TextMeshPro>();
    if (text != null)
    {
      if (useAlternativeClick)
      {
        text.text = button.additionalLabel;
      }
      else
      {
        text.text = button.label;
        text.enableWordWrapping = false;
      }
    }
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    Material material = gameObject.GetComponent<Renderer>().material;
    LeanTween.cancel(gameObject);
    material.color = button.color + new Color(0.6f, 0.6f, 0.6f);
    LeanTween.value(gameObject, (Color color) =>
    {
      material.color = color;
    }, material.color, material.color + new Color(0.2f, 0.2f, 0.2f), 0.15f).setOnComplete(() =>
    {
      if (useAlternativeClick)
      {
        button.additionalCallback();
      }
      else
      {
        button.callback();
      }
    });

  }
}
