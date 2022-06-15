using UnityEngine;

public class ColorSettings : MonoBehaviour
{
  public static ColorSettings instance;
  private void Awake()
  {
    instance = this;
  }

  //Edges:
  //
  //Foreground 	#000 	black 	lines, borders, arrowheads, text, numbers
  public Color defaultEdgeColor;
  // Indirect Highlighting 	#f90 	orange 	rectangles, circles
  [ColorUsage(true, true)]
  public Color edgeSelectedColor;
  // Hover  #fff   white lines
  [ColorUsage(true, true)]
  public Color edgeHoverColor;
  // Highlighting 	#f00 	red lines, arrowheads
  public Color edgeGrabbedColor;


  //Nodes
  public Color defaultNodeColor;
  public Color uriColor;
  public Color variableColor;
  public Color blankNodeColor;
  // Indirect Highlighting 	#f90 	orange 	nodes
  [ColorUsage(true, true)]
  public Color nodeSelectedColor;
  // Hover  #fff   white nodes
  [ColorUsage(true, true)]
  public Color nodeHoverColor;
  //Highlighting 	#f00 	red nodes
  public Color nodeGrabbedColor;
}
