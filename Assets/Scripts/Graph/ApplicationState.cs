using Dweiss;
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
    public int saveVersion = 1;
    public List<GraphState> graphs = new List<GraphState>();
    public List<EdgeState> crossGraphEdges = new List<EdgeState>();
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
    public EdgeState(Edge edge, bool crossGraphEdge = false)
    {
      subjectNode = new NodeState(edge.displaySubject);
      objectNode = new NodeState(edge.displayObject);
      predicate = edge.uri;
      isVariable = edge.IsVariable;
      isSelected = edge.IsSelected;
      if (crossGraphEdge)
      {
        optionalObjectGraphGUID = edge.displayObject.graph.GUID;
        optionalSubjectGraphGUID = edge.displaySubject.graph.GUID;
      }
    }
    public NodeState subjectNode;
    public string predicate;
    public NodeState objectNode;
    public bool isVariable = false;
    public bool isSelected = false;
    public string optionalObjectGraphGUID = "";
    public string optionalSubjectGraphGUID = "";
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
      Texture2D texture = node.GetTexture();
      if (texture != null)
      {

        image = ClampTextureSize(texture, Settings.Instance.savedMaximumImageWidth, Settings.Instance.savedMaximumImageHeight).EncodeToPNG();
        imageWidth = texture.width;
        imageHeight = texture.height;
      }
    }

    private Texture2D ClampTextureSize(Texture2D source, int targetWidth, int targetHeight)
    {
      int scaleWidth = 0;
      int scaleHeight = 0;
      float aspect = (float)source.width / source.height;
      if (source.width > targetWidth)
      {
        scaleWidth = targetWidth;
        scaleHeight = (int)(targetHeight / aspect);
        if (scaleHeight > targetHeight)
        {
          scaleWidth = (int)(targetWidth * aspect);
          scaleHeight = targetHeight;
        }
        return ScaleTexture(source, scaleWidth, scaleHeight);
      }
      return source;
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
      Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
      float incX = (1.0f / (float)targetWidth);
      float incY = (1.0f / (float)targetHeight);
      for (int i = 0; i < result.height; ++i)
      {
        for (int j = 0; j < result.width; ++j)
        {
          Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
          result.SetPixel(j, i, newColor);
        }
      }
      result.Apply();
      return result;
    }

    public float positionX;
    public float positionY;
    public float positionZ;
    public string uri;
    public string label;
    public string cachedNodeLabel;
    public bool isVariable;
    public bool isLocked;
    public byte[] image;
    public int imageWidth;
    public int imageHeight;
    public string nodeOfExternalGraphGUID = "";
  }

  private static State SaveState()
  {
    State state = new State();
    GameObject[] graphs = GameObject.FindGameObjectsWithTag("Graph");
    foreach (GameObject graph in graphs)
    {
      GraphState graphData = SaveGraphState(graph.GetComponent<Graph>(), state);
      state.graphs.Add(graphData);
    }
    return state;
  }

  private static GraphState SaveGraphState(Graph graph, State mainState)
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
      bool isCrossGraphEdge = edge.graph.GUID != edge.displayObject.graph.GUID || edge.graph.GUID != edge.displaySubject.graph.GUID;
      if (isCrossGraphEdge)
      {
        mainState.crossGraphEdges.Add(new EdgeState(edge, true));
      }
      else
      {
        state.edges.Add(new EdgeState(edge));
      }
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

  static List<Graph> graphs = new List<Graph>();
  private static void LoadState(State state)
  {
    graphs.Clear();
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

    // handle cross graph nodes
    foreach (EdgeState edge in state.crossGraphEdges)
    {
      LoadEdgeState(edge);
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

  private static Edge LoadEdgeState(EdgeState state, Graph graph = null)
  {
    Node nodeSubject;
    Node nodeObject;
    if (graph == null)
    {
      Graph subjectGraph = graphs.Find((Graph graphCheck) => graphCheck.GUID == state.optionalSubjectGraphGUID);
      Graph objectGraph = graphs.Find((Graph graphCheck) => graphCheck.GUID == state.optionalObjectGraphGUID);
      nodeSubject = LoadNodeState(state.subjectNode, subjectGraph);
      nodeObject = LoadNodeState(state.objectNode, objectGraph);
      graph = subjectGraph;
    }
    else
    {
      nodeSubject = LoadNodeState(state.subjectNode, graph);
      nodeObject = LoadNodeState(state.objectNode, graph);
    }

    if (nodeSubject == null || nodeObject == null) return null;
    Edge edge = graph.CreateEdge(nodeSubject, state.predicate, nodeObject);
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
    if (state.image != null)
    {
      Texture2D image = new Texture2D(state.imageWidth, state.imageHeight);
      image.LoadImage(state.image);
      node.SetTexture(image, state.imageWidth, state.imageHeight);
    }
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
