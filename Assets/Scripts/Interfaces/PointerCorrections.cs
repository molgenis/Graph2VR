using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerCorrections : MonoBehaviour
{
  public Vector3 questOffset = Vector3.zero;
  public Vector3 questRotation = Vector3.zero;

  public Vector3 viveOffset = Vector3.zero;
  public Vector3 viveRotation = Vector3.zero;

  void Start()
  {
    ControllerType.instance.GetControllerName((string name) =>
    {
      if (name == "quest")
      {
        transform.localPosition = questOffset;
        transform.rotation = Quaternion.Euler(questRotation);
      }
      else if (name == "vive")
      {
        transform.localPosition = viveOffset;
        transform.rotation = Quaternion.Euler(viveRotation);
      }
    });
  }
}
