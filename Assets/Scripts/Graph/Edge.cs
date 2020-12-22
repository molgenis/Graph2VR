using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    public string uri;
    public Node from;
    public Node to;

    private LineRenderer lineRenderer;
    private TMPro.TextMeshPro textFront;
    private TMPro.TextMeshPro textBack;

    private void Start()
    {
        transform.localPosition = (from.transform.localPosition + to.transform.localPosition) * 0.5f;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;

        Vector3 fromPosition = from.transform.localPosition - transform.localPosition;
        Vector3 toPosition = to.transform.localPosition - transform.localPosition;
        lineRenderer.SetPosition(0, fromPosition);
        lineRenderer.SetPosition(1, toPosition);

        TMPro.TextMeshPro textFront = transform.Find("FrontText").GetComponent<TMPro.TextMeshPro>();
        TMPro.TextMeshPro textBack = transform.Find("BackText").GetComponent<TMPro.TextMeshPro>();
        textFront.text = textBack.text = uri;

        // Calculate Text rotations
        Vector2 rot = CalculateAngles(fromPosition, toPosition, true);
        textFront.transform.localRotation = Quaternion.Euler(0, rot.x, rot.y);
        textFront.transform.localPosition = textFront.transform.localRotation * (Vector3.up * 0.05f);
        rot = CalculateAngles(fromPosition, toPosition, false);
        textBack.transform.localRotation = Quaternion.Euler(0, rot.x, rot.y);
        textBack.transform.localPosition = textBack.transform.localRotation * (Vector3.up * 0.05f);
    }

    private Vector2 CalculateAngles(Vector3 fromPosition, Vector3 toPosition, bool isFront)
    {
        float height = (fromPosition.y - toPosition.y);
        float angle = -90;
        if (isFront) {
            height = (toPosition.y - fromPosition.y); 
            angle = 90;
        }
        float yRotation = angle + Mathf.Atan2(fromPosition.x, fromPosition.z) * (180 / Mathf.PI);
        float zRotation = Mathf.Asin(height / Vector3.Distance(fromPosition, toPosition)) * (180 / Mathf.PI);
        return new Vector2(yRotation, zRotation);
    }
}
