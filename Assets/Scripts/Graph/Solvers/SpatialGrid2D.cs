using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialGrid2D : BaseLayoutAlgorithm
{
  public float pushDistance = 0.5f;
  public float pullDistance = 1;

  public float pushForce = 1;
  public float pullForce = 1;
  public float centerForce = 0.1f;

  public float left = -10;
  public float right = 10;
  public float top = 10;
  public float bottom = 0;

  public float drag = 0.95f;
  List<Node>[,] list;

  private int xSteps;
  private int ySteps;

  public float effectPower = 1;

  private void Start()
  {
    xSteps = (int)(((-left) + right) / pushDistance);
    ySteps = (int)(((-bottom) + top) / pushDistance);
    list = new List<Node>[xSteps, ySteps];
    for (int y = 0; y < ySteps; y++)
    {
      for (int x = 0; x < xSteps; x++)
      {
        list[x, y] = new List<Node>();
      }
    }
    InvokeRepeating("ReorderList", 1, 1);
  }

  private void ReorderList()
  {
    if (effectPower > 0)
    {

      // Clear lookup list
      for (int y = 0; y < ySteps; y++)
      {
        for (int x = 0; x < xSteps; x++)
        {
          list[x, y].Clear();
        }
      }

      // put nodes in lookup list
      foreach (Node node in graph.nodeList)
      {
        // Add Spatial properties
        if (node.gameObject.GetComponent<Spatial>() == null) node.gameObject.AddComponent<Spatial>();

        // local node space to lookup list space;
        Vector2Int pos = LocalSpaceToListSpace(node.transform.localPosition);
        Get(pos.x, pos.y).Add(node);
      }
    }
  }

  private List<Node> Get(int x, int y)
  {
    return list[Mathf.Clamp(x, 0, xSteps - 1), Mathf.Clamp(y, 0, ySteps - 1)];
  }

  List<Node> GetNeighbours(Vector2Int position)
  {
    List<Node> neighbours = new List<Node>();
    neighbours.AddRange(Get(position.x - 1, position.y));
    neighbours.AddRange(Get(position.x + 1, position.y));
    neighbours.AddRange(Get(position.x, position.y - 1));
    neighbours.AddRange(Get(position.x, position.y + 1));
    neighbours.AddRange(Get(position.x - 1, position.y - 1));
    neighbours.AddRange(Get(position.x + 1, position.y + 1));
    neighbours.AddRange(Get(position.x + 1, position.y - 1));
    neighbours.AddRange(Get(position.x - 1, position.y + 1));
    return neighbours;
  }

  private Vector2Int LocalSpaceToListSpace(Vector3 position)
  {
    int x = (int)((position.x / pushDistance) + left);
    int y = (int)((position.y / pushDistance) + bottom);
    return new Vector2Int(Mathf.Clamp(x, 0, xSteps), Mathf.Clamp(y, 0, ySteps));
  }

  private void Update()
  {
    if (effectPower > 0)
    {
      effectPower = effectPower - (0.1f * Time.deltaTime);
      foreach (Node node in graph.nodeList)
      {
        Spatial spatial = node.GetComponent<Spatial>();
        if (spatial == null) continue;
        // push away from sides

        // push close nodes away
        foreach (Node neighbour in GetNeighbours(LocalSpaceToListSpace(node.transform.localPosition)))
        {
          if (neighbour != null && !Object.Equals(node, neighbour))
          {
            float distance = Vector3.Distance(node.transform.localPosition, neighbour.transform.localPosition);
            if (distance < pushDistance)
            {
              float force = -(1 - (distance / pushDistance)) * pushForce * Time.deltaTime;
              Vector3 normal = (neighbour.transform.localPosition - node.transform.localPosition).normalized;
              spatial.force += normal * force;
            }
          }
        }

        // pull along edges
        foreach (Node connected in node.connections)
        {
          if (connected != null && !Object.Equals(node, connected))
          {
            float distance = Vector3.Distance(node.transform.localPosition, connected.transform.localPosition);
            if (distance > pullDistance)
            {
              float force = (1 - (1 / distance)) * pullForce * Time.deltaTime;
              Vector3 normal = (connected.transform.localPosition - node.transform.localPosition).normalized;
              spatial.force += normal * force;
            }
          }
        }

        // small move to center 
        spatial.force += -node.transform.localPosition.normalized * centerForce * Time.deltaTime;

        // slow all forces
        spatial.force *= drag; // FIXME: this is not framerate independent!

        // apply forces and clamp
        float x = Mathf.Clamp(node.transform.localPosition.x, left, right);
        float y = Mathf.Clamp(node.transform.localPosition.y, bottom, top);
        x += spatial.force.x * effectPower;
        y += spatial.force.y * effectPower;

        node.transform.localPosition = new Vector3(x, y, 0);
      }
    }
  }

  public override void CalculateLayout()
  {
    effectPower = 1;
  }

  public override void Stop()
  {
    effectPower = 0;
  }
}
