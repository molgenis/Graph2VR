using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplaySelectedNodeInMenu : MonoBehaviour
{
  Node node;
  Edge edge;
  public GameObject displayNode;
  public TextMeshPro textNode;
  public GameObject displayEdge;
  public TextMeshPro textEdge;

  public GameObject glow;

  private void Start()
  {
    InvokeRepeating("DisableCheck", 0.2f, 0.2f);
  }

  void PopulateNode(UnityEngine.Object input)
  {
    node = input as Node;
    textNode.text = node.label;
    displayNode.SetActive(true);
    //glow.GetComponent<Renderer>().material = null;
    displayNode.GetComponent<Renderer>().material.color = ColorSettings.instance.nodeGrabbedColor;
  }

  void PopulateEdge(UnityEngine.Object input)
  {
    edge = input as Edge;
    textEdge.text = edge.uri;
    displayEdge.SetActive(true);
    //glow.GetComponent<Renderer>().material = null;
    displayEdge.GetComponent<Renderer>().material.color = ColorSettings.instance.nodeGrabbedColor;
  }

  void DisableCheck()
  {
    if (node != null)
    {
      if (!node.IsActiveInMenu)
      {
        textNode.text = "";
        displayNode.SetActive(false);
        //glow.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
      }
    }
    if (edge != null)
    {
      if (!edge.IsActiveInMenu)
      {
        textEdge.text = "";
        displayEdge.SetActive(false);
        //glow.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
      }
    }
  }
}
