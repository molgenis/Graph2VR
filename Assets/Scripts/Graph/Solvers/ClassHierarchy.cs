using System.Collections.Generic;
using UnityEngine;

public class ClassHierarchy : BaseLayoutAlgorithm
{
   private float offsetSize = 0.3f;
   private string subClassOfPredicate = "http://www.w3.org/2000/01/rdf-schema#subclassof";

   public override void CalculateLayout()
   {
      ResetNodes();
      List<Edge> subClassOfEdgeList = graph.edgeList.FindAll(edge => edge.uri.ToLower() == subClassOfPredicate);
      Node initialNode = null;
      if (subClassOfEdgeList.Count == 0)
      {
         Debug.Log("Not a class hiearchy");
         return;
      }
      else
      {
         initialNode = subClassOfEdgeList[0].displaySubject;
      }

      initialNode.SetHierarchicalLevel(0);
      SetHierarchicalLayers(initialNode);

      // Special case: Multiple root nodes
      foreach (Node node in graph.nodeList)
      {
         if (!node.hierarchicalLevelFound)
         {
            node.SetHierarchicalLevel(0);
            SetHierarchicalLayers(node);
         }
      }

      // Set positions
      float offset = 0;
      foreach (Node node in graph.nodeList)
      {
         if (node.hierarchicalLevel == 0)
         {
            offset += PositionNodeLayer(node, 0, offset);
         }
      }
   }

   private void ResetNodes()
   {
      foreach (Node node in graph.nodeList)
      {
         node.hierarchicalLevelFound = false;
         node.hierarchicalPositionSet = false;
      }
   }

   public float PositionNodeLayer(Node node, int layer, float offset)
   {
      float newOffset = offset;
      if (!node.LockPosition)
      {
         node.transform.localPosition = new Vector3(0, 0, offset) + new Vector3(layer * (offsetSize * 2), 0, 0);
         newOffset += 0.1f;
      }
      else
      {
         //offset = node.transform.localPosition - new Vector3(layer * (offsetSize * 2), 0, 0);
      }

      node.hierarchicalPositionSet = true;
      int nextLayer = layer + 1;
      //int previousLayer = layer - 1;

      foreach (Edge edge in node.connections)
      {
         Node childNode;

         // Are we a object or subject?
         if (node.graph.RealNodeValue(node.graphNode) == node.graph.RealNodeValue(edge.graphSubject))
         {
            childNode = edge.displayObject;
         }
         else
         {
            childNode = edge.displaySubject;
         }

         if (childNode.hierarchicalLevel == nextLayer && !childNode.hierarchicalPositionSet)
         {
            newOffset = PositionNodeLayer(childNode, nextLayer, newOffset);
         }
      }
      return newOffset;
   }

   /*
   List<string> structurePredicats = new List<string>
   {
      "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://www.w3.org/2000/01/rdf-schema#subclassof"
   };
   */

   private void SetHierarchicalLayers(Node node)
   {
      List<Node> nodesToCall = new List<Node>();
      foreach (Edge edge in node.connections)
      {
         if (edge.uri == subClassOfPredicate)
         {
            int objectSubjectOrderDirection;

            Node other;
            if (node.graph.RealNodeValue(node.graphNode) == node.graph.RealNodeValue(edge.graphSubject))
            {
               other = edge.displayObject; // We are a subject
               objectSubjectOrderDirection = -1;
            }
            else
            {
               other = edge.displaySubject; // we are a object
               objectSubjectOrderDirection = 1;
            }

            if (!other.hierarchicalLevelFound)
            {
               other.SetHierarchicalLevel(node.hierarchicalLevel + objectSubjectOrderDirection);
               nodesToCall.Add(other);
            }
         }
      }
      foreach (Node n in nodesToCall)
      {
         SetHierarchicalLayers(n);
      }

      // Get lowest level
      int lowestLevel = int.MaxValue;
      foreach (Node currentNode in graph.nodeList)
      {
         if (currentNode.hierarchicalLevel < lowestLevel) lowestLevel = currentNode.hierarchicalLevel;
      }

      // Correct lowels level to 0
      foreach (Node currentNode in graph.nodeList)
      {
         currentNode.hierarchicalLevel = (currentNode.hierarchicalLevel - lowestLevel);
      }
   }

   public override void Stop()
   {
   }
}
