using UnityEngine;

public class BoundingSphere : MonoBehaviour
{
  public Graph graph;
  public float size = 1;
  public bool isFlat = false;
  public void Start()
  {
    transform.SetParent(null);
  }

  public void Update()
  {
    // Calc center
    Vector3 center = Vector3.zero;
    if (graph.nodeList.Count > 0)
    {
      foreach (Node node in graph.nodeList)
      {
        center += node.transform.position;
      }
      center = center / graph.nodeList.Count;

      Vector3 farpoint = Vector3.zero;
      float far = 0f;

      // Calc farpoint from center
      foreach (Node node in graph.nodeList)
      {
        float d = Vector3.Distance(center, node.transform.position);
        if (d > far)
        {
          far = d;
          farpoint = node.transform.position;
        }
      }
      size = Vector3.Distance(farpoint, center);
      transform.position = center;
      transform.localScale = new Vector3(1, 1, isFlat ? 0.1f : 1f) * size * 2;
    }
  }
}
