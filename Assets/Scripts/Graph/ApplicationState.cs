using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

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
    public string GUID;
    public float positionX;
    public float positionY;
    public float positionZ;
    public float scaleX;
    public float scaleY;
    public float scaleZ;
    public float rotationX;
    public float rotationY;
    public float rotationZ;
    public bool showBoundingSphere;
    public ushort layout;
    public string creationQuery;

    //SemanticPlanes
    public float semanticPlanesLookDirectionX = 0;
    public float semanticPlanesLookDirectionY = 0;
    public float semanticPlanesLookDirectionZ = 0;

    public string parentGraphGUID;
    public List<string> subGraphGUIDs = new List<string>();

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
    state.GUID = graph.GUID;
    state.positionX = graph.transform.position.x;
    state.positionY = graph.transform.position.y;
    state.positionZ = graph.transform.position.z;
    state.scaleX = graph.transform.localScale.x;
    state.scaleY = graph.transform.localScale.y;
    state.scaleZ = graph.transform.localScale.z;
    state.rotationX = graph.transform.rotation.eulerAngles.x;
    state.rotationY = graph.transform.rotation.eulerAngles.y;
    state.rotationZ = graph.transform.rotation.eulerAngles.z;
    state.creationQuery = graph.creationQuery;
    state.showBoundingSphere = graph.boundingSphere.IsVisible();
    state.layout = ((ushort)graph.GetLayout());

    SemanticPlanes plane = graph.GetComponent<SemanticPlanes>();
    Quaternion direction = plane.lookDirection;
    state.semanticPlanesLookDirectionX = direction.eulerAngles.x;
    state.semanticPlanesLookDirectionY = direction.eulerAngles.y;
    state.semanticPlanesLookDirectionZ = direction.eulerAngles.z;

    state.parentGraphGUID = (graph.parentGraph == null) ? "" : graph.parentGraph.GUID;
    foreach (Graph subGraph in graph.subGraphs)
    {
      state.subGraphGUIDs.Add(subGraph.GUID);
    }

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
    List<Graph> graphs = new List<Graph>();
    foreach (GraphState graphState in state.graphs)
    {
      graphs.Add(LoadGraphState(graphState));
    }

    // handle self references
    foreach (Graph graph in graphs)
    {
      if (graph.graphState.parentGraphGUID != "")
      {
        graph.parentGraph = graphs.Find((Graph graphCheck) => graphCheck.GUID == graph.graphState.parentGraphGUID);
      }

      foreach (string guid in graph.graphState.subGraphGUIDs)
      {
        graph.subGraphs.Add(graphs.Find((Graph graphCheck) => graphCheck.GUID == guid));
      }

      SemanticPlanes plane = graph.GetComponent<SemanticPlanes>();
      plane.parentGraph = graph.parentGraph;// graphs.Find((Graph graph) => graph.GUID == graph.graphState.semanticPlanesParentGraphGUID);
      graph.boundingSphere.lookDirection = plane.lookDirection;
    }
  }

  private static Graph LoadGraphState(GraphState state)
  {
    Graph graph = Main.instance.CreateGraph();
    graph.graphState = state;
    graph.GUID = state.GUID;
    graph.transform.position = new Vector3(state.positionX, state.positionY, state.positionZ);
    graph.transform.localScale = new Vector3(state.scaleX, state.scaleY, state.scaleZ);
    graph.transform.rotation = Quaternion.Euler(state.rotationX, state.rotationY, state.rotationZ);
    graph.creationQuery = state.creationQuery;
    graph.SetLayout((Graph.Layout)state.layout);

    SemanticPlanes plane = graph.GetComponent<SemanticPlanes>();
    plane.lookDirection = Quaternion.Euler(state.semanticPlanesLookDirectionX, state.semanticPlanesLookDirectionY, state.semanticPlanesLookDirectionZ);

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
    return graph;
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
