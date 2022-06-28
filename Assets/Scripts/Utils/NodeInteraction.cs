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
      Node lastSelection = null;
      foreach (Node graphNode in graph.nodeList)
      {
         if (graphNode.IsActiveInMenu) lastSelection = graphNode;
         graphNode.IsActiveInMenu = false;
      }
      Node node = GetComponent<Node>();
      node.IsActiveInMenu = true;
      if (node != lastSelection)
      {
         GameObject.FindGameObjectWithTag("LeftController").BroadcastMessage("Close", SendMessageOptions.DontRequireReceiver);
         GameObject.FindGameObjectWithTag("LeftController").BroadcastMessage("PopulateNode", node, SendMessageOptions.DontRequireReceiver);
      }
   }

   void IGrabInterface.ControllerEnter()
   {
      node.IsControllerHovered = true;
   }

   void IGrabInterface.ControllerExit()
   {
      node.IsControllerHovered = false;
   }

   void IGrabInterface.ControllerGrabBegin(GameObject newParent)
   {
      node.IsControllerGrabbed = true;

      this.transform.SetParent(newParent.transform, true);
      graph.layout.Stop();
   }

   void IGrabInterface.ControllerGrabEnd()
   {
      node.IsControllerGrabbed = false;

      this.transform.SetParent(originalParent, true);
      // NOTE: make some button to trigger the solver again. We dont always want it to solve. Sometimes we want the move the nodes ourself
      // graph.layout.CalculateLayout(); 
   }

   void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
   {
      node.IsPointerHovered = true;
   }

   void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
   {
      node.IsPointerHovered = false;
   }
}
