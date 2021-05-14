using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS.RDF;

public class Edge : MonoBehaviour
{
    public string uri;
    public Node from;
    public Node to;
    public INode iNode;

    private LineRenderer lineRenderer;
    private TMPro.TextMeshPro textFront;
    private TMPro.TextMeshPro textBack;

    public string GetURI(string value)
    {
        return this.uri;
    }

    public System.Uri GetURI()
    {
        return VDS.RDF.UriFactory.Create(this.uri);
    }

    private void Start()
    {
        transform.localPosition = (from.transform.localPosition + to.transform.localPosition) * 0.5f;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;

        Vector3 fromPosition = from.transform.localPosition - transform.localPosition;
        Vector3 toPosition = to.transform.localPosition - transform.localPosition;

        textFront = transform.Find("FrontText").GetComponent<TMPro.TextMeshPro>();
        textBack = transform.Find("BackText").GetComponent<TMPro.TextMeshPro>();
        textFront.text = textBack.text = uri;

        // Calculate Text rotations
        fromPosition = from.transform.localPosition - transform.localPosition;
        toPosition = to.transform.localPosition - transform.localPosition;
        Vector2 rot = CalculateAngles(fromPosition, toPosition, true);
        textFront.transform.localRotation = Quaternion.Euler(0, rot.x, rot.y);
        textFront.transform.localPosition = textFront.transform.localRotation * (Vector3.up * 0.05f);
        rot = CalculateAngles(fromPosition, toPosition, false);
        textBack.transform.localRotation = Quaternion.Euler(0, rot.x, rot.y);
        textBack.transform.localPosition = textBack.transform.localRotation * (Vector3.up * 0.05f);
    }

    private Vector2 CalculateAngles(Vector3 fromPosition, Vector3 toPosition, bool isFront)
    {
        if(Vector3.Distance(fromPosition, toPosition) == 0) {
            return Vector2.zero;
        }
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

    private void Update()
    {
        if(from == null || to == null) {
            Debug.Log("WAAAAAAa dont be null");
            return;
        }
        transform.position = (from.transform.position + to.transform.position) * 0.5f;
        Vector3 fromPosition = from.transform.position - transform.position;
        Vector3 toPosition = to.transform.position - transform.position;
        lineRenderer.SetPosition(0, transform.worldToLocalMatrix * fromPosition);
        lineRenderer.SetPosition(1, transform.worldToLocalMatrix * toPosition);
        Vector2 rot = CalculateAngles(fromPosition, toPosition, true);
        textFront.transform.rotation = Quaternion.Euler(0, rot.x, rot.y); // note this is world rotation
        textFront.transform.localPosition = textFront.transform.localRotation * (Vector3.up * 0.05f); // note this is local position
        rot = CalculateAngles(fromPosition, toPosition, false);
        textBack.transform.rotation = Quaternion.Euler(0, rot.x, rot.y);
        textBack.transform.localPosition = textBack.transform.localRotation * (Vector3.up * 0.05f);
    }
}
