using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class SelectControllerModel : MonoBehaviour
{
  public GameObject quest;
  public GameObject vive;

  private void Start()
  {
    ControllerType.instance.GetControllerName((string name) =>
    {
      if (name == "quest")
      {
        Instantiate(quest, transform);
      }
      else
      {
        Instantiate(vive, transform);
      }
    });
  }
}
