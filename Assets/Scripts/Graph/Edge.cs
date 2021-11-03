using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS.RDF;

public class Edge : MonoBehaviour
{
    public string uri;
    public Node from;
    public Node to;
    public INode iFrom;
    public INode iNode;
    public INode iTo;
    public Transform arrow;
    public new CapsuleCollider collider;
    private LineRenderer lineRenderer;
    private TMPro.TextMeshPro textFront;
    private TMPro.TextMeshPro textBack;

    public bool isVariable = false;
    public bool isSelected = false;
    public bool isPointerHovered = false;
    public bool isControllerHovered = false;
    public bool isControllerGrabbed = false;

    public Color defaultColor;
    public Color selectedColor;
    public Color hoverColor;
    public Color grabbedColor;

    private string textShort;
    private string textLong;

    private void Update()
    {
        // TODO: trigger a update function, dont do this every frame.
        if (isControllerHovered || isPointerHovered) {
            SetColor(hoverColor);
        }else if (isControllerGrabbed) {
            SetColor(grabbedColor);
        } else if(isSelected) {
            SetColor(selectedColor);
        } else {
            SetColor(defaultColor);
        }

        if (from == null || to == null) {
            return;
        }
        UpdatePosition();
    }


    public void Select()
    {
        isSelected = true;


        Graph.instance.AddToSelection(new Graph.Triple {
            Subject = iFrom.ToString(),
            Predicate = iNode.ToString(),
            Object = iTo.ToString()
        });
    }

    public void Deselect()
    {
        isSelected = false;

        // TODO: This needs to be fixed, probibly a check by value not reverence?
        Graph.instance.RemoveFromSelection(new Graph.Triple {
            Subject = iFrom.ToString(),
            Predicate = iNode.ToString(),
            Object = iTo.ToString()
        });
    }

    public bool Equals(INode Subject, INode Predicate, INode Object)
    {
        return Subject.Equals(iFrom) && Predicate.Equals(iNode) && Object.Equals(iTo);
    }

    public string GetURI(string value)
    {
        return this.uri;
    }

    public System.Uri GetURI()
    {
        return VDS.RDF.UriFactory.Create(this.uri);
    }

    public void SetDefaultColor(Color color)
    {
        defaultColor = color;
        SetColor(color);
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
        collider = transform.Find("Collider").GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        transform.localPosition = (from.transform.localPosition + to.transform.localPosition) * 0.5f;

        Vector3 fromPosition = from.transform.localPosition - transform.localPosition;
        Vector3 toPosition = to.transform.localPosition - transform.localPosition;

        textFront = transform.Find("FrontText").GetComponent<TMPro.TextMeshPro>();
        textBack = transform.Find("BackText").GetComponent<TMPro.TextMeshPro>();

        string qname = Graph.instance.GetShortName(uri);
        if (qname != "") {
            textShort = textLong = qname;
        } else {
            textShort = URIEnd(uri);
            textLong = uri;
        }

        // Calculate Text rotations
        UpdatePosition();
    }

    private string URIEnd(string uri)
    {
        var list = uri.Split('/', '#');
        if(list.Length > 0) {
            return list[list.Length-1];
        }
        return uri;
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

    private void UpdatePosition()
    {
        transform.position = (from.transform.position + to.transform.position) * 0.5f;
        Vector3 fromPosition = from.transform.position - transform.position;
        Vector3 toPosition = to.transform.position - transform.position;

        float distance = ((toPosition - fromPosition).magnitude);
        float textDistance = (distance * (1 / textBack.transform.localScale.x)) * 0.8f;
        Vector3 normal = (toPosition - fromPosition).normalized;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.005f * transform.lossyScale.magnitude;
        lineRenderer.SetPosition(0, transform.worldToLocalMatrix * fromPosition);
        lineRenderer.SetPosition(1, transform.worldToLocalMatrix * (toPosition - (normal * ((to.transform.lossyScale.x * 0.5f) + (arrow.lossyScale.x * 0.05f)))));
        Vector2 rot = CalculateAngles(fromPosition, toPosition, true);
        textFront.transform.rotation = Quaternion.Euler(0, rot.x, rot.y); // note this is world rotation
        textFront.transform.localPosition = textFront.transform.localRotation * (Vector3.up * 0.025f); // note this is local position
        rot = CalculateAngles(fromPosition, toPosition, false);
        textBack.transform.rotation = Quaternion.Euler(0, rot.x, rot.y);
        textBack.transform.localPosition = textBack.transform.localRotation * (Vector3.up * 0.025f);
        textBack.rectTransform.sizeDelta = new Vector2(textDistance, 1);
        textFront.rectTransform.sizeDelta = new Vector2(textDistance, 1);
        arrow.localPosition = (transform.worldToLocalMatrix * (toPosition - (normal * (to.transform.lossyScale.x * 0.5f)))) ;
        arrow.rotation = Quaternion.FromToRotation(Vector3.up, normal);

        // Position the collider
        collider.transform.rotation = Quaternion.Euler(0, rot.x, rot.y);
        collider.transform.localPosition = Vector3.zero;
        collider.height = distance;

        // Update text
        if (isPointerHovered || isControllerHovered || isControllerGrabbed) {
            textFront.text = textBack.text = textLong;
        } else {
            textFront.text = textBack.text = textShort;
        }
    }
}