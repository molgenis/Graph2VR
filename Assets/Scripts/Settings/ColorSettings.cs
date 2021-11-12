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

//Edges:
//
//Foreground 	#000 	black 	lines, borders, arrowheads, text, numbers
    public Color defaultEdgeColor;
// Indirect Highlighting 	#f90 	orange 	rectangles, circles
  public Color edgeSelectedColor;
// Hover  #fff   white lines
  public Color edgeHoverColor;
// Highlighting 	#f00 	red lines, arrowheads
  public Color edgeGrabbedColor;


//Nodes
  public Color defaultNodeColor;
  public Color uriColor;
// Datatype 	#fc3 	yellow 	rdfs:Datatype, rdfs:Literal
  public Color literalColor;
  public Color variableColor;
  public Color blankNodeColor;
// Indirect Highlighting 	#f90 	orange 	nodes
  public Color nodeSelectedColor;
// Hover  #fff   white nodes
  public Color nodeHoverColor;
//Highlighting 	#f00 	red nodes
  public Color nodeGrabbedColor;



// VOWL color schema
//ToDo: Implement those
// General 	#acf 	light blue 	owl:Class, owl:ObjectProperty (incl. subclasses)
  public Color nodeOwlClassColor;
// Rdf 	#c9c 	light purple 	rdfs:Class, rdfs:Resource, rdf:Property
  public Color nodeRdfsClassColor;
// Datatype Property 	#9c6 	light green owl:DatatypeProperty
  public Color nodeOwlDatatypeColor;
// Deprecated 	#ccc 	light gray 	owl:DeprecatedClass, owl:DeprecatedProperty
  public Color DeprecatedColor;
    // Neutral 	#fff 	white   owl:Thing, arrowhead of rdfs:subClassOf
  public Color arrowheadSubclassOfColor;
}
