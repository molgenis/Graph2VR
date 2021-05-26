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
    public Transform arrow;

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

    public void SetColor(Color color)
    {
        lineRenderer.material.color = color;
        arrow.GetComponent<Renderer>().material.color = color;
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        arrow = transform.Find("Arrow");
    }

    private void Start()
    {
        transform.localPosition = (from.transform.localPosition + to.transform.localPosition) * 0.5f;

        Vector3 fromPosition = from.transform.localPosition - transform.localPosition;
        Vector3 toPosition = to.transform.localPosition - transform.localPosition;

        textFront = transform.Find("FrontText").GetComponent<TMPro.TextMeshPro>();
        textBack = transform.Find("BackText").GetComponent<TMPro.TextMeshPro>();
        textFront.text = textBack.text = uri;

        // Calculate Text rotations
        UpdatePosition();
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
            return;
        }
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = (from.transform.position + to.transform.position) * 0.5f;
        Vector3 fromPosition = from.transform.position - transform.position;
        Vector3 toPosition = to.transform.position - transform.position;
        Vector3 normal = (toPosition - fromPosition).normalized;
        lineRenderer.SetPosition(0, transform.worldToLocalMatrix * fromPosition);
        lineRenderer.SetPosition(1, transform.worldToLocalMatrix * (toPosition - (normal * ((to.transform.localScale.x * 0.5f) + (arrow.localScale.x * 0.05f)))));
        Vector2 rot = CalculateAngles(fromPosition, toPosition, true);
        textFront.transform.rotation = Quaternion.Euler(0, rot.x, rot.y); // note this is world rotation
        textFront.transform.localPosition = textFront.transform.localRotation * (Vector3.up * 0.05f); // note this is local position
        rot = CalculateAngles(fromPosition, toPosition, false);
        textBack.transform.rotation = Quaternion.Euler(0, rot.x, rot.y);
        textBack.transform.localPosition = textBack.transform.localRotation * (Vector3.up * 0.05f);

        
        arrow.localPosition = (transform.worldToLocalMatrix * (toPosition - (normal * (to.transform.localScale.x * 0.5f)))) ;
        //arrow.localPosition = toPosition - (normal * 0.15f);
        arrow.rotation = Quaternion.FromToRotation(Vector3.up, normal);
    }
}
