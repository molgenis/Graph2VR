using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EdgeInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IGrabInterface
{
  private Edge edge;

  public void Start()
  {
    edge = GetComponent<Edge>();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    Edge edge = GetComponent<Edge>();
    GameObject.FindGameObjectWithTag("LeftController").BroadcastMessage("PopulateEdge", edge, SendMessageOptions.DontRequireReceiver);
  }


  void IGrabInterface.ControllerEnter()
  {
    edge.IsControllerHovered = true;
  }

  void IGrabInterface.ControllerExit()
  {
    edge.IsControllerHovered = false;
  }

  void IGrabInterface.ControllerGrabBegin(GameObject newParent)
  {
    edge.IsControllerGrabbed = true;
  }

  void IGrabInterface.ControllerGrabEnd()
  {
    edge.IsControllerGrabbed = false;
  }

  void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
  {
    edge.IsPointerHovered = true;
  }

  void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
  {
    edge.IsPointerHovered = false;
  }
}
