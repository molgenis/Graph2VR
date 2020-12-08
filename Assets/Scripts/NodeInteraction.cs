using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Canvas menu;

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

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = new Color(1, 1, 1);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = new Color(0, 0.259f, 0.6f);
    }
}
