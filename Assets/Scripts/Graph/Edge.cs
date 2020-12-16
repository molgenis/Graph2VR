using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    public string uri;
    public Node from;
    public Node to;
    private LineRenderer lineRenderer;
    private void Start()
    {
        transform.localPosition = (from.transform.localPosition + to.transform.localPosition) * 0.5f;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;

        Vector3 fromPosition = from.transform.localPosition - transform.localPosition;
        Vector3 toPosition = to.transform.localPosition - transform.localPosition;
        lineRenderer.SetPosition(0, fromPosition);
        lineRenderer.SetPosition(1, toPosition);

        TMPro.TextMeshPro text = GetComponentInChildren<TMPro.TextMeshPro>(true);
        text.text = uri;
        text.transform.localRotation = Quaternion.FromToRotation(Vector3.right, fromPosition.normalized);
        text.transform.localPosition = text.transform.localRotation * (Vector3.up * 0.08f);

    }

    void Update()
    {

    }

}
