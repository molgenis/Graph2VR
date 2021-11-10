using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class NodeMenu : MonoBehaviour
{
    private CircleMenu cm = null;
    public bool isOutgoingLink = true;
    private Dictionary<string, System.Tuple<string, int>> set;
    private Node node = null;
    private Edge edge = null;

    public GameObject controlerModel;
    public SteamVR_Action_Boolean clickAction = null;

    public void Start()
    {
        cm = GetComponent<CircleMenu>();
    }

    public void GetPredicats()
    {
        if (node != null) {
            if (isOutgoingLink) {
                set = Graph.instance.GetOutgoingPredicats(node.GetURIAsString());
            } else {
                set = Graph.instance.GetIncomingPredicats(node.GetURIAsString());
            }
        }
    }

    public void Update()
    {
        if (clickAction.GetStateDown(SteamVR_Input_Sources.LeftHand) == true) {
            Close();
        }
    }

    public void PopulateNode(Object input)
    {
        KeyboardHandler.instance.Close();
        node = input as Node;
        if (node.isVariable) {
            Close();
            controlerModel.SetActive(false);
            cm.AddButton("Undo conversion", Color.blue / 2, () => {
                node.UndoConversion();
                PopulateNode(input);
            });
            cm.AddButton("Rename", Color.red / 2, () => { KeyboardHandler.instance.Open(node); });
            cm.ReBuild();
        } else {
            GetPredicats();
            Close();
            controlerModel.SetActive(false);

            if (set != null) {
                if (isOutgoingLink) {
                    cm.AddButton("List incoming predicts", Color.blue / 2, () => {
                        isOutgoingLink = false;
                        PopulateNode(input);
                    });
                } else {
                    cm.AddButton("List outgoing predicts", Color.blue / 2, () => {
                        isOutgoingLink = true;
                        PopulateNode(input);
                    });
                }

                foreach (KeyValuePair<string, System.Tuple<string, int>> item in set) {
                    //Debug.Log("k: " + item.Key + " v1: " + item.Value.Item1 + " v2: " + item.Value.Item2);
                    Color color = Color.gray;
                    string label = item.Value.Item1;
                    if (label == "") {
                        label = item.Key;
                        color = Color.gray * 0.75f;
                    }
                    // TODO: add qname als alt.

                    cm.AddButton(label, color, () => {
                        Graph.instance.ExpandGraph(node, item.Key, isOutgoingLink);
                        Close();
                    }, item.Value.Item2);
                }
            }

            if (!node.isVariable) {
                cm.AddButton("Convert to Variable", Color.blue / 2, () => {
                    node.MakeVariable();
                    PopulateNode(input);
                });
            }

            if (node.uri != "") {
                cm.AddButton("Collapse Incoming", new Color(1, 0.5f, 0.5f) / 2, () => {
                    Graph.instance.CollapseIncomingGraph(node);
                });
                cm.AddButton("Collapse Outgoing", new Color(1, 0.5f, 0.5f) / 2, () => {
                    Graph.instance.CollapseOutgoingGraph(node);
                });
                cm.AddButton("Collapse All", new Color(1, 0.5f, 0.5f) / 2, () => {
                    Graph.instance.CollapseGraph(node);
                });
            }

            cm.AddButton("Close", Color.red / 2, () => {
                Graph.instance.RemoveNode(node);
                Close();
            });

            cm.ReBuild();
        }
    }

    public void PopulateEdge(Object input)
    {
        KeyboardHandler.instance.Close();
        edge = input as Edge;
        if (edge.isVariable) {
            Close();
            controlerModel.SetActive(false);
            cm.AddButton("Undo conversion", Color.blue / 2, () => {
                edge.UndoConversion();
                PopulateEdge(input);
            });

            if (edge.isSelected) {
                cm.AddButton("Remove selection", Color.yellow / 2, () => {
                    edge.Deselect();
                    PopulateEdge(input);
                });
                cm.AddButton("Query similar patterns", Color.yellow / 2, () => {
                    Graph.instance.QuerySimilarPatterns();
                });
            } else {
                cm.AddButton("Select triple", Color.yellow / 2, () => {
                    edge.Select();
                    PopulateEdge(input);
                });
            }
            cm.AddButton("Rename", Color.red / 2, () => { KeyboardHandler.instance.Open(edge); });
            cm.ReBuild();
        } else {
            Close();
            controlerModel.SetActive(false);

            if (!edge.isVariable) {
                cm.AddButton("Convert to Variable", Color.blue / 2, () => {
                    edge.MakeVariable();
                    PopulateEdge(input);
                });
            }

            if (edge.isSelected) {
                cm.AddButton("Remove selection", Color.yellow / 2, () => {
                    edge.Deselect();
                    PopulateEdge(input);
                });
                cm.AddButton("Query similar patterns", Color.yellow / 2, () => {
                    Graph.instance.QuerySimilarPatterns();
                });

            } else {
                cm.AddButton("Select triple", Color.yellow / 2, () => {
                    edge.Select();
                    PopulateEdge(input);
                });
            }

            cm.ReBuild();
        }
    }

    public void Close()
    {
        if (cm != null) {
            cm.Close();
            KeyboardHandler.instance.Close();
            controlerModel.SetActive(true);
        }
    }
}
