using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class SelectControlerModel : MonoBehaviour
{
  public bool useQuest = true;
  public GameObject quest;
  public GameObject vive;

  private void Start()
  {
    if (useQuest)
    {
      Instantiate(quest, transform);
    }
    else
    {
      Instantiate(vive, transform);
    }
  }
}
