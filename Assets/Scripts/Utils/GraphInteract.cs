using Dweiss;
using UnityEngine;
using UnityEngine.InputSystem;

public class GraphInteract : MonoBehaviour
{
  private GameObject CurrentHoveredObject = null;
  private GameObject GrabbedObject = null;

  private bool IsHoldingPinchButton = false;
  private float HoldBeginTime;
  private LineRenderer lineRenderer;
  private bool IsDraggingLine = false;
  private Node EdgeBegin = null;
  public bool isLeftController = true;
  private static int nodeCreationCounter = 1;
  private static int edgeCreationCounter = 1;

  void Start()
  {
    if (isLeftController)
    {
      ControlerInput.instance.triggerActionLeft.action.performed += Trigger;
      ControlerInput.instance.gripActionLeft.action.performed += Grip;
    }
    else
    {
      ControlerInput.instance.triggerActionRight.action.performed += Trigger;
      ControlerInput.instance.gripActionRight.action.performed += Grip;
    }

    lineRenderer = gameObject.AddComponent<LineRenderer>();
    lineRenderer.material = Resources.Load<Material>("Materials/line");
    lineRenderer.enabled = false;
    lineRenderer.useWorldSpace = true;
    lineRenderer.startWidth = 0.01f;
    lineRenderer.endWidth = 0.01f;
  }

  // Update is called once per frame
  void Update()
  {
    if (Main.instance.mainGraph == null) return;

    if (IsDraggingLine)
    {
      lineRenderer.SetPosition(1, transform.position);
    }
    else if (IsHoldingPinchButton)
    {
      if (HoldBeginTime + 2 < Time.time)
      {
        CreateNewNode();
      }
    }

    Collider[] overlapping = Physics.OverlapSphere(transform.position, 0.03f);
    GameObject closestObject = null;
    foreach (Collider col in overlapping)
    {
      GameObject colliderAsGrab = null;
      if (col.gameObject.GetComponent<IGrabInterface>() != null)
      {
        colliderAsGrab = col.gameObject;
      }
      if (colliderAsGrab != null)
      {
        if (closestObject != null)
        {
          if (Vector3.SqrMagnitude(transform.position - col.gameObject.transform.position) < Vector3.SqrMagnitude(transform.position - closestObject.transform.position))
          {
            closestObject = colliderAsGrab;
          }
        }
        else
        {
          closestObject = colliderAsGrab;
        }
      }
    }

    HandleHoveredObject(closestObject);
  }

  void CreateNewNode()
  {
    IsHoldingPinchButton = false;
    Node node = Main.instance.mainGraph.CreateNode(Settings.Instance.DefaultNodeCreationURI + nodeCreationCounter, transform.position);
    nodeCreationCounter++;
    node.MakeVariable();
    Main.instance.mainGraph.nodeList.Add(node);
  }

  void HandleHoveredObject(GameObject newHoveredObject)
  {
    IGrabInterface newGrabAble = null;
    if (newHoveredObject)
    {
      newGrabAble = newHoveredObject.GetComponent<IGrabInterface>();
    }
    if (newGrabAble == null)
    {
      if (CurrentHoveredObject != null)
      {
        CurrentHoveredObject.GetComponent<IGrabInterface>().ControllerExit();
        CurrentHoveredObject = null;
      }
    }
    else
    {
      if (newHoveredObject != CurrentHoveredObject)
      {
        if (CurrentHoveredObject)
        {
          CurrentHoveredObject.GetComponent<IGrabInterface>().ControllerExit();
        }
        newGrabAble.ControllerEnter();
        CurrentHoveredObject = newHoveredObject;
      }
    }
  }

  bool oldGrip = false;
  private void Grip(InputAction.CallbackContext a)
  {
    bool newState = a.ReadValueAsButton();
    if (newState != oldGrip)
    {
      if (newState)
      {
        if (CurrentHoveredObject)
        {
          CurrentHoveredObject.GetComponent<IGrabInterface>().ControllerGrabBegin(this.gameObject);
          GrabbedObject = CurrentHoveredObject;
        }
      }
      else
      {
        if (GrabbedObject)
        {
          GrabbedObject.GetComponent<IGrabInterface>().ControllerGrabEnd();
          GrabbedObject = null;
        }
      }
    }
    oldGrip = newState;
  }

  bool oldTrigger = false;
  private void Trigger(InputAction.CallbackContext a)
  {
    bool newState = a.ReadValueAsButton();
    if (newState != oldTrigger)
    {
      if (newState)
      {
        if (CurrentHoveredObject)
        {
          EdgeBegin = CurrentHoveredObject.GetComponent<Node>();
          if (EdgeBegin)
          {
            IsDraggingLine = true;
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, CurrentHoveredObject.transform.position);
          }
        }
        else
        {
          IsHoldingPinchButton = true;
          HoldBeginTime = Time.time;
        }
      }
      else
      {
        IsHoldingPinchButton = false;
        IsDraggingLine = false;
        lineRenderer.enabled = false;
        if (CurrentHoveredObject)
        {
          Node EdgeEnd = CurrentHoveredObject.GetComponent<Node>();
          if (EdgeBegin != null && EdgeEnd != null && EdgeBegin != EdgeEnd)
          {
            Edge edge = EdgeBegin.graph.CreateEdge(EdgeBegin, Settings.Instance.DefaultEdgeCreationURI + edgeCreationCounter, EdgeEnd);
            edgeCreationCounter++;
          }
        }
        EdgeBegin = null;
      }
    }
    oldTrigger = newState;
  }
}
