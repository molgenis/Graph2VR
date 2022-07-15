using System;
using UnityEngine;

[Serializable]
public class Hierarchical
{
   public enum HierarchicalType { SubClassOf, Type, Other, None }

   public void Reset()
   {
      level = 0;
      levelFound = false;
      positionSet = false;
      parent = null;
      typeCount = 0;
      otherCount = 0;
      offset = 0;
      typeWithChildNodes = false;
      targetLocation = Vector3.zero;
      hierarchicalType = HierarchicalType.None;
   }
   public int level = 0;
   public bool levelFound = false;
   public bool positionSet = false;
   public Node parent = null;
   public int typeCount = 0;
   public int otherCount = 0;
   public float offset = 0;
   public bool typeWithChildNodes = false;
   //public bool outGoingNode = false;
   public Vector3 targetLocation = Vector3.zero;
   public HierarchicalType hierarchicalType = HierarchicalType.None;
}