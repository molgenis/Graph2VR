using System.Collections.Generic;
using UnityEngine;

public class ClassHierarchy : BaseLayoutAlgorithm
{
   private float offsetSize = 0.3f;
   private string subClassOfPredicate = "http://www.w3.org/2000/01/rdf-schema#subclassof";
   private string typePredicate = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";
   private string owlThing = "http://www.w3.org/2002/07/owl#Thing";
   private bool running = false;

   private void Update()
   {
      if (running)
      {
         foreach (Node node in graph.nodeList)
         {
            node.transform.localPosition = Vector3.Lerp(node.transform.localPosition, node.hierarchicalSettings.targetLocation, Time.deltaTime * 2);
         }
      }
   }

   public override void CalculateLayout()
   {
      ResetNodes();
      graph.SortNodes();
      CalculateHierarchicalLevels();
      SortNodeList();
      CalculatePositions();
      running = true;
   }

   private void ResetNodes()
   {
      foreach (Node node in graph.nodeList)
      {
         node.hierarchicalSettings.Reset();
      }
   }

   private void CalculateHierarchicalLevels()
   {
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
      SetHierarchicalLevels(initialNode);

      // Special case: Multiple root nodes
      foreach (Node node in graph.nodeList)
      {
         if (!node.hierarchicalSettings.levelFound)
         {
            node.SetHierarchicalLevel(0);
            SetHierarchicalLevels(node);
         }
      }
   }

   private void CalculatePositions()
   {
      float offset = 0;
      foreach (Node node in graph.nodeList)
      {
         if (node.hierarchicalSettings.level == 0)
         {
            offset = PositionNodeLayer(node, 0, offset);
         }
      }
   }

   private void SortNodeList()
   {
      foreach (Node node in graph.nodeList)
      {
         node.connections.Sort((Edge a, Edge b) => string.Compare(a.displaySubject.textMesh.text, b.displaySubject.textMesh.text));
      }
   }

   private void SetHierarchicalLevels(Node node)
   {
      List<Node> nodesToCall = new List<Node>();
      foreach (Edge edge in node.connections)
      {
         int objectSubjectOrderDirection;

         Node other;
         if (node.graph.RealNodeValue(node.graphNode) == node.graph.RealNodeValue(edge.graphSubject))
         {
            other = edge.displayObject; // We are a subject

            if (edge.uri.ToLower() == subClassOfPredicate)
            {
               objectSubjectOrderDirection = -1;
            }
            else
            {
               objectSubjectOrderDirection = 1;
            }
            /*
            if (other.uri == owlThing)
            {
               objectSubjectOrderDirection = 1;
            }
            */
         }
         else
         {
            other = edge.displaySubject; // we are a object
            objectSubjectOrderDirection = 1;
         }

         if (edge.uri.ToLower() == subClassOfPredicate)
         {

            if (!other.hierarchicalSettings.levelFound)
            {
               other.hierarchicalSettings.hierarchicalType = Hierarchical.HierarchicalType.SubClassOf;
               if (objectSubjectOrderDirection == 1)
                  other.hierarchicalSettings.parent = node;
               other.hierarchicalSettings.typeCount = 0;
               other.hierarchicalSettings.otherCount = 0;
               other.SetHierarchicalLevel(node.hierarchicalSettings.level + objectSubjectOrderDirection);
               nodesToCall.Add(other);
            }
         }
         else if (edge.uri.ToLower() == typePredicate)
         {

            if (!other.hierarchicalSettings.levelFound)
            {
               if (objectSubjectOrderDirection == 1)
               {
                  other.hierarchicalSettings.hierarchicalType = Hierarchical.HierarchicalType.Type;
               }
               else
               {
                  other.hierarchicalSettings.hierarchicalType = Hierarchical.HierarchicalType.Other;
               }
               if (objectSubjectOrderDirection == 1)
                  other.hierarchicalSettings.parent = node;
               other.hierarchicalSettings.typeCount = 0;
               other.hierarchicalSettings.otherCount = 0;
               other.SetHierarchicalLevel(node.hierarchicalSettings.level + objectSubjectOrderDirection);
               nodesToCall.Add(other);
            }
         }
         else
         {
            if (!other.hierarchicalSettings.levelFound)
            {
               other.hierarchicalSettings.hierarchicalType = Hierarchical.HierarchicalType.Other;
               other.hierarchicalSettings.parent = node;
               other.hierarchicalSettings.parent.hierarchicalSettings.typeWithChildNodes = true;
               other.hierarchicalSettings.otherCount = 0;
               other.SetHierarchicalLevel(node.hierarchicalSettings.level + objectSubjectOrderDirection);
               nodesToCall.Add(other);
            }
         }

      }
      foreach (Node n in nodesToCall)
      {
         SetHierarchicalLevels(n);
      }

      // Get lowest level
      int lowestLevel = int.MaxValue;
      foreach (Node currentNode in graph.nodeList)
      {
         if (currentNode.hierarchicalSettings.level < lowestLevel) lowestLevel = currentNode.hierarchicalSettings.level;
      }

      // Correct lowels level to 0
      foreach (Node currentNode in graph.nodeList)
      {
         currentNode.hierarchicalSettings.level = (currentNode.hierarchicalSettings.level - lowestLevel);
      }
   }

   public float PositionNodeLayer(Node node, int layer, float offset)
   {
      float newOffset = offset;
      if (!node.LockPosition)
      {
         int typeDepth = 0;
         int otherDepth = 0;
         if (node.hierarchicalSettings.hierarchicalType == Hierarchical.HierarchicalType.Type && node.hierarchicalSettings.parent != null)
         {
            node.hierarchicalSettings.parent.hierarchicalSettings.typeCount++;
            typeDepth = node.hierarchicalSettings.parent.hierarchicalSettings.typeCount;
            offset = node.hierarchicalSettings.parent.hierarchicalSettings.offset;
            if (node.hierarchicalSettings.typeWithChildNodes)
            {
               node.hierarchicalSettings.parent.hierarchicalSettings.typeCount++;
            }
         }
         if (node.hierarchicalSettings.hierarchicalType == Hierarchical.HierarchicalType.Other && node.hierarchicalSettings.parent != null)
         {
            node.hierarchicalSettings.parent.hierarchicalSettings.otherCount++;
            if (node.hierarchicalSettings.parent.hierarchicalSettings.parent != null)
            {
               typeDepth = node.hierarchicalSettings.parent.hierarchicalSettings.parent.hierarchicalSettings.typeCount;
            }
            otherDepth = node.hierarchicalSettings.parent.hierarchicalSettings.otherCount;
            offset = node.hierarchicalSettings.parent.hierarchicalSettings.offset;
         }

         node.hierarchicalSettings.targetLocation = new Vector3(0, typeDepth * offsetSize, offset) + new Vector3((layer * (offsetSize * 2)) + (otherDepth * offsetSize), 0, 0);
         node.hierarchicalSettings.offset = offset;
         if (node.hierarchicalSettings.hierarchicalType == Hierarchical.HierarchicalType.SubClassOf)
         {
            newOffset += offsetSize;
         }
      }

      node.hierarchicalSettings.positionSet = true;
      int nextLayer = layer + 1;

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

         if (childNode.hierarchicalSettings.level == nextLayer && !childNode.hierarchicalSettings.positionSet)
         {
            newOffset = PositionNodeLayer(childNode, nextLayer, newOffset);
         }
      }
      return newOffset;
   }

   public override void Stop()
   {
      running = false;
   }
}
