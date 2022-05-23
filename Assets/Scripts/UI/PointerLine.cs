using UnityEngine;

public class PointerLine : MonoBehaviour
{
  public int maxDistance = 10;
  public LineRenderer line;
  public Transform endPoint;
  public LayerMask layer;
  void Update()
  {
    RaycastHit hit;
    Vector3 endpoint = transform.position + (transform.forward * maxDistance);
    if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, layer.value))
    {
      endpoint = hit.point;
    }
    line.SetPosition(0, Vector3.zero);
    line.SetPosition(1, transform.InverseTransformPoint(endpoint));
    endPoint.transform.position = endpoint;

  }
}
