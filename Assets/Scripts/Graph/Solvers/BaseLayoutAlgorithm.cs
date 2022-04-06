using UnityEngine;

public class BaseLayoutAlgorithm : MonoBehaviour
{
  protected Graph graph;
  public Graph parentGraph = null;

  public void Awake()
  {
    graph = GetComponent<Graph>();
  }

  public virtual void CalculateLayout()
  {

  }

  public virtual void Stop()
  {

  }
}
