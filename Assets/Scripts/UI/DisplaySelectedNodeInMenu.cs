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
    glow.SetActive(false);
    displayNode.GetComponent<Renderer>().material.color = ColorSettings.instance.nodeGrabbedColor;
  }

  void PopulateEdge(UnityEngine.Object input)
  {
    edge = input as Edge;
    textEdge.text = edge.uri;
    displayEdge.SetActive(true);
    glow.SetActive(false);
    displayEdge.GetComponent<Renderer>().material.color = ColorSettings.instance.nodeGrabbedColor;
  }

  void DisableCheck()
  {

    if (node != null)
    {
      if (!node.IsActiveInMenu)
      {
        textNode.text = "";
        node = null;
        if (edge == null) glow.SetActive(true);
        displayNode.SetActive(false);
      }
    }
    if (edge != null)
    {
      if (!edge.IsActiveInMenu)
      {
        textEdge.text = "";
        edge = null;
        if (node == null) glow.SetActive(true);
        displayEdge.SetActive(false);
      }
    }

    if (edge == null && node == null)
    {
      displayNode.SetActive(false);
      displayEdge.SetActive(false);
      glow.SetActive(true);
    }
  }
}
