using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;

public class GraphInteract : MonoBehaviour
{
    public SteamVR_Action_Boolean pinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
    public SteamVR_Input_Sources inputSource;

    public SpawnGraph graph;
    GameObject lastHit = null;
    // Start is called before the first frame update

    public Canvas menu;
    private Vector3 direction = new Vector3(0, -0.5f, 2);
    void Start()
    {
        pinchAction[inputSource].onChange += SteamVR_Behaviour_Pinch_OnChange;
        LineRenderer line = gameObject.AddComponent<LineRenderer>();
        Material loaded = AssetDatabase.LoadAssetAtPath<Material>("Assets/Graph/line.mat");
        line.material = loaded;
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        line.useWorldSpace = false;
        line.SetPosition(0, new Vector3(0, 0, 0));
        line.SetPosition(1, direction);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitinfo;
        int layerMask = 1 << 8 | 1 << 5;
        if (Physics.Raycast(transform.localPosition, transform.rotation * direction, out hitinfo, Mathf.Infinity, layerMask))
        {
            if (lastHit != hitinfo.collider.gameObject && graph.nodes.Contains(hitinfo.collider.gameObject))
            {
                MeshRenderer meshRenderer = hitinfo.collider.gameObject.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(1, 1, 1);
                if(lastHit)
                {
                    MeshRenderer meshRenderer2 = lastHit.GetComponent<MeshRenderer>();
                    meshRenderer2.material.color = new Color(1, 0, 1);
                }
                lastHit = hitinfo.collider.gameObject;
            }
            //hitinfo.collider.gameObject
        } else if(lastHit)
        {
            MeshRenderer meshRenderer = lastHit.GetComponent<MeshRenderer>();
            meshRenderer.material.color = new Color(1, 0, 1);
            lastHit = null;
        }
    }

    private void SteamVR_Behaviour_Pinch_OnChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
    {
        if(newState)
        {
            if (lastHit)
            {
                menu.enabled = true;
                menu.transform.position = lastHit.transform.position;
                menu.transform.rotation = Camera.main.transform.rotation;
                menu.transform.position += menu.transform.rotation * new Vector3(0.15f, 0, 0);

                TMPro.TextMeshProUGUI text = GameObject.Find("UI_Title").GetComponent<TMPro.TextMeshProUGUI>();
                if(text)
                {
                    text.text = lastHit.GetComponentInChildren<TMPro.TextMeshPro>().text;
                }
            }
            else
            {
                menu.enabled = false;
            }
        }
    }
}