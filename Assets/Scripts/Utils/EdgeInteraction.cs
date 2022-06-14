using UnityEngine;
using UnityEngine.EventSystems;

public class EdgeInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IGrabInterface
{
  private Edge edge;
  private Graph graph;

  public void Start()
  {
    edge = GetComponent<Edge>();
    graph = transform.parent.GetComponent<Graph>();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    Edge lastSelection = null;
    if (graph == null) return;

    foreach (Edge graphEdge in graph.edgeList)
    {
      if (graphEdge.IsActiveInMenu) lastSelection = graphEdge;
      graphEdge.IsActiveInMenu = false;
    }
    Edge edge = GetComponent<Edge>();
    edge.IsActiveInMenu = true;

    if (edge != lastSelection)
    {
      GameObject.FindGameObjectWithTag("LeftController").BroadcastMessage("Clear", SendMessageOptions.DontRequireReceiver);
      GameObject.FindGameObjectWithTag("LeftController").BroadcastMessage("PopulateEdge", edge, SendMessageOptions.DontRequireReceiver);
    }
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
