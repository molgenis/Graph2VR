using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingSphere : MonoBehaviour
{
  public Graph graph;
  public float size = 1;
  public void Start()
  {
    transform.SetParent(null);
  }
  // Update is called once per frame
  public void Update()
  {
    // Calc center
    Vector3 center = Vector3.zero;
    if (graph.nodeList.Count > 0) {
      foreach (Node node in graph.nodeList) {
        center += node.transform.position;
      }
      center = center / graph.nodeList.Count;

      Vector3 farpoint = Vector3.zero;
      float far = 0f;

      // Calc farpoint from center
      foreach (Node node in graph.nodeList) {
        float d = Vector3.Distance(center, node.transform.position);
        if (d > far) {
          far = d;
          farpoint = node.transform.position;
        }
      }
      size = Vector3.Distance(farpoint, center);
      transform.position = center;
      transform.localScale = Vector3.one * size * 2;
    }
  }
}
