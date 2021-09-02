using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;

public class GraphInteract : MonoBehaviour
{
    private SteamVR_Action_Boolean grabAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
    private SteamVR_Action_Boolean pinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
    public SteamVR_Input_Sources inputSource;

    private GameObject CurrentHoveredObject = null;
    private GameObject GrabbedObject = null;

    private bool IsHoldingPinchButton = false;
    private float HoldBeginTime;
    private LineRenderer lineRenderer;
    private bool IsDraggingLine = false;
    private Node EdgeBegin = null;

    // Start is called before the first frame update
    void Start()
    {
        grabAction[inputSource].onChange += SteamVR_Behaviour_Grab_OnChange;
        pinchAction[inputSource].onChange += SteamVR_Behaviour_Pinch_OnChange;

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
        if (Graph.instance == null) return;

        if(IsDraggingLine)
        {
            lineRenderer.SetPosition(1, transform.position);
        } 
        else if (IsHoldingPinchButton)
        {
            if (HoldBeginTime + 2 < Time.time)
            {
                IsHoldingPinchButton = false;
                Node node = Graph.instance.CreateNode("No label", transform.position);
                Graph.instance.nodeList.Add(node);
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

    private void SteamVR_Behaviour_Grab_OnChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
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

    private void SteamVR_Behaviour_Pinch_OnChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
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
                    Edge edge = Graph.instance.CreateEdge(EdgeBegin, "No label", EdgeEnd);
                    Graph.instance.edgeList.Add(edge);
                }
            }
            EdgeBegin = null;
        }
    }
}