using UnityEngine;

public class BoundingSphere : MonoBehaviour
{
  public Graph graph;
  public float size = 1;
  public bool isFlat = false;
  public Quaternion lookDirection = Quaternion.identity;
  public void Start()
  {
    transform.SetParent(null);
    gameObject.SetActive(false);
  }

  public void Update()
  {
    // Calc center
    Vector3 center = Vector3.zero;
    double xCoordinate = 0;
    double yCoordinate = 0;
    double zCoordinate = 0;

    if (graph.nodeList.Count > 0)
    {
      foreach (Node node in graph.nodeList)
      {
        xCoordinate += node.transform.position.x;
        yCoordinate += node.transform.position.y;
        zCoordinate += node.transform.position.z;
      }
      center = new Vector3(
        calculateCoordinate(xCoordinate),
        calculateCoordinate(yCoordinate),
        calculateCoordinate(zCoordinate)
       );

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
      transform.rotation = graph.transform.rotation * lookDirection;
      transform.localScale = new Vector3(1, 1, isFlat ? 0.1f : 1f) * size * 2;
    }
  }
  private float calculateCoordinate(double coordinate)
  {
    int count = graph.nodeList.Count;
    return (float)(coordinate / count);
  }

}
