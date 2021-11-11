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
    edge.isControllerHovered = true;
  }

  void IGrabInterface.ControllerExit()
  {
    edge.isControllerHovered = false;
  }

  void IGrabInterface.ControllerGrabBegin(GameObject newParent)
  {
    edge.isControllerGrabbed = true;
  }

  void IGrabInterface.ControllerGrabEnd()
  {
    edge.isControllerGrabbed = false;
  }

  void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
  {
    edge.isPointerHovered = true;
  }

  void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
  {
    edge.isPointerHovered = false;
  }
}
