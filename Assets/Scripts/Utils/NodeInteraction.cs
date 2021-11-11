using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IGrabInterface
{
  private Transform originalParent;
  private Graph graph;
  private Node node;

  public void Start()
  {
    node = GetComponent<Node>();
    originalParent = transform.parent;
    graph = originalParent.GetComponent<Graph>();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    Node node = GetComponent<Node>();
    GameObject.FindGameObjectWithTag("LeftController").BroadcastMessage("PopulateNode", node, SendMessageOptions.DontRequireReceiver);
  }

  void IGrabInterface.ControllerEnter()
  {
    node.isControllerHovered = true;
  }

  void IGrabInterface.ControllerExit()
  {
    node.isControllerHovered = false;
  }

  void IGrabInterface.ControllerGrabBegin(GameObject newParent)
  {
    node.isControllerGrabbed = true;

    this.transform.SetParent(newParent.transform, true);
    graph.layout.Stop();
  }

  void IGrabInterface.ControllerGrabEnd()
  {
    node.isControllerGrabbed = false;

    this.transform.SetParent(originalParent, true);
    graph.layout.CalculateLayout();
  }

  void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
  {
    node.isPointerHovered = true;
  }

  void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
  {
    node.isPointerHovered = false;
  }
}
