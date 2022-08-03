using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


//graphs
// - selections
// - layout algoritm
//  - edges
//  - nodes 
//    - positions
//    - pin state
//    - image / label state?

public class ApplicationState
{
  [Serializable]
  public class State
  {
    public List<GraphState> graphs = new List<GraphState>();
  }

  [Serializable]
  public class GraphState
  {
    public float positionX;
    public float positionY;
    public float positionZ;
    public float scaleX;
    public float scaleY;
    public float scaleZ;
    public float rotationX;
    public float rotationY;
    public float rotationZ;
    public float rotationW;
    public bool showBoundingSphere;
    public List<EdgeState> edges = new List<EdgeState>();
    public List<NodeState> unconnectedNodes = new List<NodeState>();
  }

  [Serializable]
  public class EdgeState
  {
    public EdgeState(Edge edge)
    {
      subjectNode = new NodeState(edge.displaySubject);
      objectNode = new NodeState(edge.displayObject);
      predicate = edge.uri;
      isVariable = edge.IsVariable;
      isSelected = edge.IsSelected;
    }
    public NodeState subjectNode;
    public string predicate;
    public NodeState objectNode;
    public bool isVariable = false;
    public bool isSelected = false;
  }

  [Serializable]
  public class NodeState
  {
    public NodeState(Node node)
    {
      positionX = node.transform.position.x;
      positionY = node.transform.position.y;
      positionZ = node.transform.position.z;
      uri = node.uri;
      label = node.label;
      isVariable = node.IsVariable;
      isLocked = node.lockPosition;
      cachedNodeLabel = node.cachedNodeLabel;
    }
    public float positionX;
    public float positionY;
    public float positionZ;
    public string uri;
    public string label;
    public string cachedNodeLabel;
    public bool isVariable;
    public bool isLocked;
  }

  private static State SaveState()
  {
    State state = new State();
    GameObject[] graphs = GameObject.FindGameObjectsWithTag("Graph");
    foreach (GameObject graph in graphs)
    {
      GraphState graphData = SaveGraphState(graph.GetComponent<Graph>());
      state.graphs.Add(graphData);
    }
    return state;
  }

  private static GraphState SaveGraphState(Graph graph)
  {
    GraphState state = new GraphState();
    state.positionX = graph.transform.position.x;
    state.positionY = graph.transform.position.y;
    state.positionZ = graph.transform.position.z;
    state.scaleX = graph.transform.localScale.x;
    state.scaleY = graph.transform.localScale.y;
    state.scaleZ = graph.transform.localScale.z;
    state.rotationX = graph.transform.rotation.x;
    state.rotationY = graph.transform.rotation.y;
    state.rotationZ = graph.transform.rotation.z;
    state.rotationY = graph.transform.rotation.w;
    state.showBoundingSphere = graph.boundingSphere.IsVisible();

    // Triples
    foreach (Edge edge in graph.edgeList)
    {
      state.edges.Add(new EdgeState(edge));
    }

    // Unconnected nodes
    foreach (Node node in graph.nodeList)
    {
      if (node.connections.Count == 0)
      {
        state.unconnectedNodes.Add(new NodeState(node));
      }
    }
    return state;
  }

  public static void Save(string filename)
  {
    State state = SaveState();
    Stream ms = File.OpenWrite(FileName(filename));
    BinaryFormatter formatter = new BinaryFormatter();
    formatter.Serialize(ms, state);
    ms.Flush();
    ms.Close();
    ms.Dispose();
  }

  public static void Load(string filename)
  {
    Main.instance.ClearWorkspace();
    BinaryFormatter formatter = new BinaryFormatter();
    Stream ms = File.OpenRead(FileName(filename));
    State state = formatter.Deserialize(ms) as State;
    ms.Close();
    LoadState(state);
  }

  private static void LoadState(State state)
  {
    foreach (GraphState graphState in state.graphs)
    {
      LoadGraphState(graphState, Main.instance.CreateGraph());
    }
  }

  private static void LoadGraphState(GraphState state, Graph graph)
  {
    graph.transform.position = new Vector3(state.positionX, state.positionY, state.positionZ);
    graph.transform.localScale = new Vector3(state.scaleX, state.scaleY, state.scaleZ);
    graph.transform.rotation = new Quaternion(state.rotationX, state.rotationW, state.rotationZ, state.rotationW);

    // Recreate edges
    foreach (EdgeState edgeState in state.edges)
    {
      LoadEdgeState(edgeState, graph);
    }

    foreach (NodeState nodeState in state.unconnectedNodes)
    {
      LoadNodeState(nodeState, graph);
    }

    if (state.showBoundingSphere)
    {
      graph.boundingSphere.Show();
    }
    else
    {
      graph.boundingSphere.Hide();
    }

    // Recreate selections
    foreach (Edge edge in graph.edgeList)
    {
      if (edge.IsSelected)
      {
        graph.selection.Add(edge);
      }
    }
  }

  private static Edge LoadEdgeState(EdgeState state, Graph graph)
  {
    Edge edge = graph.CreateEdge(
      LoadNodeState(state.subjectNode, graph),
      state.predicate,
      LoadNodeState(state.objectNode, graph)
    );
    edge.IsSelected = state.isSelected;
    if (state.isVariable)
    {
      edge.MakeVariable();
    }
    return edge;
  }

  private static Node LoadNodeState(NodeState state, Graph graph)
  {
    string nodeText = state.uri == "" ? state.label : state.uri;
    Node node = graph.CreateNode(nodeText, new Vector3(state.positionX, state.positionY, state.positionZ));
    node.LockPosition = state.isLocked;
    node.cachedNodeLabel = state.cachedNodeLabel;
    if (state.isVariable)
    {
      node.MakeVariable();
      node.SetLabel(state.label);
    }
    return node;
  }

  private static string FileName(string name)
  {
    return Path.Combine(Application.persistentDataPath, name);
  }
}
