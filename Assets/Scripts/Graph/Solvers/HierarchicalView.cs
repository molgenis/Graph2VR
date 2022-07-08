using System.Collections.Generic;
using UnityEngine;

public class HierarchicalView : BaseLayoutAlgorithm
{
   private float offsetSize = 0.3f;

   public override void CalculateLayout()
   {
      Node initialNode = graph.nodeList[0];
      initialNode.SetHierarchicalLevel(0);
      SetHierarchicalLayers(initialNode);
      /*
      foreach (Node node in graph.nodeList)
      {
         if (!node.hierarchicalLevelFound) // FIX node ipv initialNode
         {
            node.SetHierarchicalLevel(0);
            SetHierarchicalLayers(node);
         }
      }
      */

      // Get lowest level
      int lowestLevel = int.MaxValue;
      int highestLevel = int.MinValue;
      foreach (Node node in graph.nodeList)
      {
         if (node.hierarchicalLevel < lowestLevel) lowestLevel = node.hierarchicalLevel;
         if (node.hierarchicalLevel > highestLevel) highestLevel = node.hierarchicalLevel;
      }
      Debug.Log("lowestLevel: " + lowestLevel);
      Debug.Log("highestLevel: " + highestLevel);
      // Correct lowels level to 0
      foreach (Node node in graph.nodeList)
      {
         node.hierarchicalLevel = (highestLevel - lowestLevel) - (node.hierarchicalLevel - lowestLevel);
      }

      float offset = 0;
      foreach (Node node in graph.nodeList)
      {
         if (node.hierarchicalLevel == 0)
         {
            PositionNodeLayer(node, 0, new Vector3(0, offset, 0));
            offset += offsetSize;
         }
      }

   }

   public void PositionNodeLayer(Node node, int layer, Vector3 offset)
   {
      node.transform.localPosition = offset;
      node.hierarchicalPositionSet = true;

      foreach (Edge edge in node.connections)
      {
         Node childNode;
         if (node.graph.RealNodeValue(node.graphNode) == node.graph.RealNodeValue(edge.graphSubject))
         {
            childNode = edge.displayObject;
         }
         else
         {
            childNode = edge.displaySubject;
         }

         if (childNode.hierarchicalLevel == layer + 1 && !childNode.hierarchicalPositionSet)
         {
            offset = AddToOffset(layer + 1, offset);
            PositionNodeLayer(childNode, layer + 1, offset);
         }
      }

   }

   public Vector3 AddToOffset(int layer, Vector3 offset)
   {
      if (layer % 3 == 0) offset += new Vector3(0, offsetSize, 0);
      if (layer % 3 == 1) offset += new Vector3(0, 0, offsetSize);
      if (layer % 3 == 2) offset += new Vector3(offsetSize, 0, 0);
      return offset;
   }

   List<string> structurePredicats = new List<string>
   {
      "rdf:type", "rdfs:subClassOf"
   };

   private void SetHierarchicalLayers(Node node)
   {
      foreach (Edge edge in node.connections)
      {
         int direction = structurePredicats.Contains(edge.textShort) ? 1 : -1;
         Node other;
         if (node.graph.RealNodeValue(node.graphNode) == node.graph.RealNodeValue(edge.graphSubject))
         {
            other = edge.displayObject; // We are a subject
         }
         else
         {
            other = edge.displaySubject; // we are a object
         }

         if (!other.hierarchicalLevelFound)
         {
            other.SetHierarchicalLevel(node.hierarchicalLevel + direction);
            SetHierarchicalLayers(other);
         }

      }
   }

   public override void Stop()
   {
   }
}
