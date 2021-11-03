using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IGrabInterface
{
    private MeshRenderer mesh;

    private bool PointerHovered = false;
    private bool ControllerHovered = false;
    private bool ControllerGrabbed = false;

    private Transform originalParent;
    private Graph graph;
    private Node node;

    public void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        node = GetComponent<Node>();
        originalParent = transform.parent;
        graph = originalParent.GetComponent<Graph>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Node node = GetComponent<Node>();
        GameObject.FindGameObjectWithTag("LeftControler").BroadcastMessage("PopulateNode", node, SendMessageOptions.DontRequireReceiver);
    }

    void SetNewColorState()
    {
        if (ControllerGrabbed)
        {
            node.SetColor(new Color(0.5f, 1.0f, 0.5f));
        }
        else if (ControllerHovered)
        {
            node.SetColor(new Color(0.5f, 0.5f, 1));
        }
        else if(PointerHovered)
        {
            node.SetColor(new Color(1, 1, 1));
        }
        else
        {
            node.SetColor(node.defaultColor);
        }
    }

    void IGrabInterface.ControllerEnter()
    {
        ControllerHovered = true;
        node.state = Node.NodeState.CloseToControler;
        SetNewColorState();
    }

    void IGrabInterface.ControllerExit()
    {
        ControllerHovered = false;
        node.state = Node.NodeState.None;
        SetNewColorState();
    }

    void IGrabInterface.ControllerGrabBegin(GameObject newParent)
    {
        ControllerGrabbed = true;
        node.state = Node.NodeState.Grabbed;
        SetNewColorState();
        this.transform.SetParent(newParent.transform, true);
        graph.layout.Stop();
    }

    void IGrabInterface.ControllerGrabEnd()
    {
        ControllerGrabbed = false;
        node.state = Node.NodeState.None;
        SetNewColorState();
        this.transform.SetParent(originalParent, true);
        graph.layout.CalculateLayout();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        node.state = Node.NodeState.Pointed;
        PointerHovered = true;
        SetNewColorState();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        node.state = Node.NodeState.None;
        PointerHovered = false;
        SetNewColorState();
    }
}
