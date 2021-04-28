using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IGrabInterface
{
    private Canvas menu;

    private MeshRenderer mesh;

    private bool PointerHovered = false;
    private bool ControllerHovered = false;
    private bool ControllerGrabbed = false;

    private Transform originalParent;
    private Graph graph;

    public void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        originalParent = transform.parent;
        graph = originalParent.GetComponent<Graph>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject.FindGameObjectWithTag("LeftControler").BroadcastMessage("Populate", SendMessageOptions.DontRequireReceiver);
        /*
        if(menu == null)
        {
            menu = Instantiate<Canvas>(Resources.Load<Canvas>("UI/ContextMenu"));
            menu.renderMode = RenderMode.WorldSpace;
            menu.worldCamera = GameObject.Find("Controller (right)").GetComponent<Camera>();
        }
        else
        {
            menu.enabled = !menu.enabled;
        }

        ContextMenuHandler selectorHandler = menu.GetComponent<ContextMenuHandler>();

        menu.transform.position = transform.position;
        menu.transform.rotation = Camera.main.transform.rotation;
        menu.transform.position += menu.transform.rotation * new Vector3(1.0f, 0, 0) * Mathf.Max(transform.lossyScale.x, gameObject.transform.lossyScale.y);

        TMPro.TextMeshPro text = gameObject.GetComponentInChildren<TMPro.TextMeshPro>();
        if (text)
        {
            menu.GetComponentInChildren<TMPro.TextMeshProUGUI>(true).text = text.text;
        }

        selectorHandler.itemSelected += delegate(string label)
        {
            if (text)
            {
                text.text = label;
            }
        };
        */
    }

    void SetNewColorState()
    {
        if (ControllerGrabbed)
        {
            mesh.material.color = new Color(0.5f, 1.0f, 0.5f);
        }
        else if (ControllerHovered)
        {
            mesh.material.color = new Color(0.5f, 0.5f, 1);
        }
        else if(PointerHovered)
        {
            mesh.material.color = new Color(1, 1, 1);
        }
        else
        {
            mesh.material.color = new Color(0, 0.259f, 0.6f);
        }
    }

    void IGrabInterface.ControllerEnter()
    {
        ControllerHovered = true;
        SetNewColorState();
    }

    void IGrabInterface.ControllerExit()
    {
        ControllerHovered = false;
        SetNewColorState();
    }

    void IGrabInterface.ControllerGrabBegin(GameObject newParent)
    {
        ControllerGrabbed = true;
        SetNewColorState();
        this.transform.SetParent(newParent.transform, true);
        graph.Temperature = 0.0f;
    }

    void IGrabInterface.ControllerGrabEnd()
    {
        ControllerGrabbed = false;
        SetNewColorState();
        this.transform.SetParent(originalParent, true);
        this.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        graph.Temperature = 0.05f;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        PointerHovered = true;
        SetNewColorState();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        PointerHovered = false;
        SetNewColorState();
    }
}
