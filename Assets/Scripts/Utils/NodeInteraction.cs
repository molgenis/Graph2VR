using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IGrabInterface
{
    public Canvas menu;

    private MeshRenderer mesh;

    private bool PointerHovered = false;
    private bool ControllerHovered = false;
    private bool ControllerGrabbed = false;

    private Transform originalParent;

    public void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        originalParent = transform.parent;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        menu.enabled = true;
        menu.transform.position = transform.position;
        menu.transform.rotation = Camera.main.transform.rotation;
        menu.transform.position += menu.transform.rotation * new Vector3(0.25f, 0, 0);

        TMPro.TextMeshProUGUI text = GameObject.Find("UI_Title").GetComponent<TMPro.TextMeshProUGUI>();
        if (text)
        {
            text.text = GetComponentInChildren<TMPro.TextMeshPro>().text;
        }
    }

    void IGrabInterface.ControllerEnter()
    {
        ControllerHovered = true;
        mesh.material.color = new Color(0.5f, 0.5f, 1);
    }

    void IGrabInterface.ControllerExit()
    {
        ControllerHovered = false;
        ControllerGrabbed = false;
        if (PointerHovered)
        {
            mesh.material.color = new Color(1, 1, 1);
        }
        else
        {
            mesh.material.color = new Color(0, 0.259f, 0.6f);
        }
    }

    void IGrabInterface.ControllerGrabBegin(GameObject newParent)
    {
        ControllerGrabbed = true;
        mesh.material.color = new Color(0.5f, 1.0f, 0.5f);
        this.transform.SetParent(newParent.transform, true);
    }

    void IGrabInterface.ControllerGrabEnd()
    {
        ControllerGrabbed = false;
        if (ControllerHovered)
        {
            mesh.material.color = new Color(0.5f, 0.5f, 1);
        }
        else if (PointerHovered)
        {
            mesh.material.color = new Color(1, 1, 1);
        }
        this.transform.SetParent(originalParent, true);
        this.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        PointerHovered = true;
        if (!ControllerHovered && !ControllerGrabbed)
        {
            mesh.material.color = new Color(1, 1, 1);
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        PointerHovered = false;
        if (!ControllerHovered && !ControllerGrabbed)
        {
            mesh.material.color = new Color(0, 0.259f, 0.6f);
        }
    }
}
