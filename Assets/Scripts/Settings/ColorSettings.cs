using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSettings : MonoBehaviour
{
  public static ColorSettings instance;
  private void Awake()
  {
    instance = this;
  }

  public Color defaultNodeColor;
  public Color defaultEdgeColor;
  public Color edgeSelectedColor;
  public Color edgeHoverColor;
  public Color edgeGrabbedColor;

  public Color uriColor;
  public Color literalColor;
  public Color variableColor;
  public Color blankNodeColor;
}
