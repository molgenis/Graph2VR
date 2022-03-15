using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CircleMenu : MonoBehaviour
{
  public Transform leftController = null;
  public Transform rightControler = null;
  public Material baseMaterial;
  public float size = 0.1f;
  public float scaleFactor = 0.5f;
  public Vector3 localRotationOffset;
  private bool isBuild = false;
  public Color defaultColor = Color.gray;
  public int sliderPathResolution = 64;

  private LineRenderer sliderLine;
  public Material sliderLineMaterial;
  public GameObject sliderNobPrefab;
  private GameObject sliderNob;
  public float sliderValue; // [0...1]
  public int minButtons = 8;
  public int maxButtons = 16;
  private float totalCalculatedAngle = 180;
  public class CircleButton
  {
    // Settings
    public string label;
    public string hoveredLabel;
    public Color color;
    public Action callback;
    public int number = -1;

    public string additionalLabel = "";
    public Action additionalCallback;

    // Generated values
    public GameObject instance;
  }

  List<CircleButton> buttons = new List<CircleButton>();
  private void Awake()
  {
  }

  private void Start()
  {
    if (leftController == null) leftController = GameObject.FindGameObjectWithTag("LeftController").transform;
    if (rightControler == null) rightControler = GameObject.FindGameObjectWithTag("RightControler").transform;
  }

  private void Update()
  {
    if (!isBuild) return;

    // position slider nob via sliderValue
    if (sliderNob != null)
    {
      float sliderAngle = (sliderValue * 180) * (Mathf.Deg2Rad);
      sliderNob.transform.localPosition = new Vector3(Mathf.Sin(-sliderAngle), Mathf.Cos(-sliderAngle), 0) * 17;
      sliderNob.transform.localRotation = Quaternion.Euler(0, 180, 0);
      sliderNob.transform.localScale = Vector3.one * 2;
    }
  }

  public void AddButton(string label, Color color, Action callback, int number = -1)
  {
    buttons.Add(new CircleButton { label = label, hoveredLabel = label, color = color, callback = callback, number = number });
  }

  public void AddButton(string label, string hoveredLabel, Color color, Action callback, int number = -1)
  {
    buttons.Add(new CircleButton { label = label, hoveredLabel = hoveredLabel, color = color, callback = callback, number = number });
  }

  public void AddButton(string label, string hoveredLabel, Color color, Action callback, string additionalLabel, Action additionalCallback)
  {
    buttons.Add(new CircleButton { label = label, hoveredLabel = hoveredLabel, color = color, callback = callback, additionalLabel = additionalLabel, additionalCallback = additionalCallback });
  }

  public void Close()
  {
    foreach (Transform child in transform)
    {
      Destroy(child.gameObject);
    }
    sliderLine = gameObject.GetComponent<LineRenderer>();
    if (sliderLine != null)
    {
      Destroy(sliderLine);
    }

    buttons.Clear();
    isBuild = false;
    sliderValue = 0;
  }

  public float GetMenuAngle()
  {
    return -sliderValue * totalCalculatedAngle;
  }

  public void ReBuild()
  {
    transform.localRotation = Quaternion.Euler(localRotationOffset);
    // Remove all child elements
    foreach (Transform child in transform) Destroy(child.gameObject);
    transform.localScale = Vector3.one * size;

    int index = 0;

    int buttonsIn180 = Mathf.Clamp(buttons.Count, minButtons, maxButtons);
    float angleStep = (180f / buttonsIn180);

    index = 0;
    float angle = 0;

    foreach (CircleButton button in buttons)
    {
      float slice = angleStep * 0.5f * Mathf.Deg2Rad;
      angle = angle + angleStep;

      GameObject clone = new GameObject("Button-" + index);
      clone.transform.parent = transform;
      clone.transform.localPosition = Vector3.zero;
      clone.transform.localRotation = Quaternion.identity;
      clone.transform.localScale = Vector3.one;
      clone.AddComponent<CircleMenuButton>().Set(this, angle, button);
      button.instance = clone;

      // Text object
      GameObject textObject = new GameObject(button.label);
      textObject.transform.parent = clone.transform;
      TextMeshPro text = textObject.AddComponent<TextMeshPro>();
      text.text = button.label;
      text.fontSizeMax = 8;
      text.fontSizeMin = 4;
      text.enableWordWrapping = false;
      text.overflowMode = TextOverflowModes.Ellipsis;
      text.enableAutoSizing = true;
      text.enableCulling = true;
      text.alignment = TextAlignmentOptions.Left;

      RectTransform textTransform = (RectTransform)textObject.transform;
      textTransform.pivot = new Vector2(0, 0.5f);
      textTransform.sizeDelta = new Vector2(7f, 1.5f);
      textTransform.localScale = new Vector3(1, 1, 1);
      textTransform.localPosition = new Vector3(0, 2.5f, 0.1f);
      textTransform.localRotation = Quaternion.Euler(0, 180, 90);

      MeshRenderer render = clone.AddComponent<MeshRenderer>();
      render.material = baseMaterial;
      render.material.color = button.color;

      MeshFilter mesh = clone.AddComponent<MeshFilter>();
      mesh.mesh = GenerateArcButton(slice, 2, 10, 0.2f);

      MeshCollider collider = clone.AddComponent<MeshCollider>();
      collider.sharedMesh = mesh.mesh;
      collider.convex = true;

      // additional object
      if (button.number != -1 || button.additionalLabel != "")
      {
        GameObject additionalButton = new GameObject("additional-button-" + index);
        additionalButton.transform.parent = clone.transform;
        additionalButton.transform.localPosition = Vector3.zero;
        additionalButton.transform.localRotation = Quaternion.identity;
        additionalButton.transform.localScale = Vector3.one;

        GameObject additionalButtonObject = new GameObject(button.number.ToString());
        additionalButtonObject.transform.parent = additionalButton.transform;
        TextMeshPro displayText = additionalButtonObject.AddComponent<TextMeshPro>();

        if (button.additionalLabel != "")
        {
          additionalButton.AddComponent<CircleMenuButton>().Set(this, angle, button, true);
          displayText.text = button.additionalLabel;
        }
        else
        {
          displayText.text = button.number.ToString();
        }

        displayText.fontSizeMax = 10;
        displayText.fontSizeMin = 6;
        displayText.enableAutoSizing = true;
        displayText.alignment = TextAlignmentOptions.Center;

        RectTransform additionalObjectTransform = (RectTransform)additionalButtonObject.transform;
        additionalObjectTransform.pivot = new Vector2(0, 0.5f);
        additionalObjectTransform.sizeDelta = new Vector2(2.6f, 3f);
        additionalObjectTransform.localScale = new Vector3(1, 1, 1);
        additionalObjectTransform.localPosition = new Vector3(0, 10.3f, 0.1f);
        additionalObjectTransform.localRotation = Quaternion.Euler(0, 180, 90);

        MeshRenderer nrender = additionalButton.AddComponent<MeshRenderer>();
        nrender.material = baseMaterial;
        nrender.material.color = button.color;

        MeshFilter nmesh = additionalButton.AddComponent<MeshFilter>();
        nmesh.mesh = GenerateArcButton(slice, 10.3f, 13.3f, 0.2f);

        MeshCollider ncollider = additionalButton.AddComponent<MeshCollider>();
        ncollider.sharedMesh = nmesh.mesh;
        ncollider.convex = true;
      }
      index++;
    }
    totalCalculatedAngle = angle - 180;

    if (totalCalculatedAngle > 0)
    {
      sliderNob = Instantiate(sliderNobPrefab, transform);
      sliderNob.GetComponent<CircleMenuSliderNob>().Set(this);

      sliderLine = gameObject.GetComponent<LineRenderer>();
      if (sliderLine == null)
      {
        sliderLine = gameObject.AddComponent<LineRenderer>();
      }

      // We need a slider
      float totalAngle = 180;
      sliderLine.positionCount = sliderPathResolution;
      sliderLine.widthMultiplier = size * 0.5f;
      sliderLine.material = sliderLineMaterial;
      sliderLine.material.color = defaultColor;
      sliderLine.useWorldSpace = false;

      // Set slider track
      for (int i = 0; i < sliderPathResolution; i++)
      {
        float sliderAngle = -(i / (float)sliderPathResolution) * totalAngle * Mathf.Deg2Rad;
        sliderLine.SetPosition(i, new Vector2(Mathf.Sin(sliderAngle), Mathf.Cos(sliderAngle)) * 17);
      }
    }
    else
    {
      sliderLine = gameObject.GetComponent<LineRenderer>();
      if (sliderLine != null)
      {
        Destroy(sliderLine);
      }
    }
    isBuild = true;
  }

  private Mesh GenerateArcButton(float angle, float from, float to, float height)
  {
    Vector3 normalA = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
    Vector3 normalB = new Vector3(Mathf.Sin(-angle), Mathf.Cos(-angle), 0);

    Mesh mesh = new Mesh();
    List<Vector3> vertices = new List<Vector3>();
    List<int> tris = new List<int>();

    //Top bottom
    vertices.Add(normalA * from + new Vector3(0, 0, -height));
    vertices.Add(normalA * to + new Vector3(0, 0, -height));
    vertices.Add(normalB * from + new Vector3(0, 0, -height));
    vertices.Add(normalB * to + new Vector3(0, 0, -height));
    vertices.Add(normalA * from);
    vertices.Add(normalA * to);
    vertices.Add(normalB * from);
    vertices.Add(normalB * to);

    //sides
    vertices.Add(normalA * from);
    vertices.Add(normalA * to);
    vertices.Add(normalA * from + new Vector3(0, 0, -height));
    vertices.Add(normalA * to + new Vector3(0, 0, -height));
    vertices.Add(normalB * from);
    vertices.Add(normalB * to);
    vertices.Add(normalB * from + new Vector3(0, 0, -height));
    vertices.Add(normalB * to + new Vector3(0, 0, -height));

    //Front back
    vertices.Add(normalB * from);
    vertices.Add(normalA * from);
    vertices.Add(normalB * from + new Vector3(0, 0, -height));
    vertices.Add(normalA * from + new Vector3(0, 0, -height));
    vertices.Add(normalB * to);
    vertices.Add(normalA * to);
    vertices.Add(normalB * to + new Vector3(0, 0, -height));
    vertices.Add(normalA * to + new Vector3(0, 0, -height));

    tris.AddRange(new int[] {
            0, 2, 1, 2, 3, 1, 5 ,7 ,6 ,5 ,6, 4,
            8+0, 8+2, 8+1, 8+2, 8+3, 8+1, 8+5 ,8+7 ,8+6 ,8+5 ,8+6, 8+4,
            16+0, 16+2, 16+1, 16+2, 16+3, 16+1, 16+5 ,16+7 ,16+6 ,16+5 ,16+6, 16+4
        });
    mesh.vertices = vertices.ToArray();
    mesh.triangles = tris.ToArray();
    mesh.RecalculateBounds();
    mesh.RecalculateNormals();
    return mesh;
  }
}
