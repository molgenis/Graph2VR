using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

public class CircleMenu : MonoBehaviour
{
    public Transform leftControler = null;
    public Transform rightControler = null;
    //public GameObject cursorPrefab = null;
    public Material baseMaterial;
    public float size = 0.1f;
    public float scaleFactor = 0.5f;
    public enum Type { Circle, HalfCircleLeft, HalfCircleRight }
    public Type type = Type.Circle;
    private bool isBuild = false;

    public SteamVR_Action_Boolean gripAction = null;

    //private LookAtTransform lookAt = null;
    //private GameObject cursorLeft = null;
    //private GameObject cursorRight = null;

    public class CircleButton
    {
        // Settings
        public string label;
        public Color color;
        public Action callback;
        public int number;

        // Generated values
        public GameObject instance;
    }

    List<CircleButton> buttons = new List<CircleButton>();

    private void Start()
    {
        //lookAt = gameObject.AddComponent<LookAtTransform>();
        //lookAt.flipDirection = true;
        if (leftControler == null) leftControler = GameObject.FindGameObjectWithTag("LeftControler").transform;
        if (rightControler == null) rightControler = GameObject.FindGameObjectWithTag("RightControler").transform;
    }

    private void Update()
    {
        if (!isBuild) return;

        // Where are the controlers pointing?
        //Plane plane = new Plane(lookAt.normal, transform.position);
        Plane plane = new Plane(Vector3.forward, transform.position);
        Ray left = new Ray(leftControler.position, leftControler.forward);
        Ray right = new Ray(rightControler.position, rightControler.forward);
        Vector3 leftPoint = Vector3.zero;
        Vector3 rightPoint = Vector3.zero;
        bool leftActive = false;
        bool rightActive = false;

        // Find menu cursor points
        float leftDistance = 0;
        if (plane.Raycast(left, out leftDistance)) {
            leftPoint = left.GetPoint(leftDistance);
            if(transform.InverseTransformPoint(leftPoint).magnitude * size * scaleFactor < 10 ) {
                leftActive = true;
            }
        }

        float rightDistance = 0;
        if(plane.Raycast(right, out rightDistance)) {
            rightPoint = right.GetPoint(rightDistance);
            if (transform.InverseTransformPoint(rightPoint).magnitude * size * scaleFactor < 10) {
                rightActive = true;
            }
        }
        
        // Scale buttons with distance
        CircleButton selectedLeftButton = null;
        CircleButton selectedRightButton = null;
        float findLeftLargest = 0;
        float findRightLargest = 0;
        foreach (CircleButton button in buttons) {
            button.instance.transform.localScale = Vector3.one;
        }
        if (leftActive) {
            foreach (CircleButton button in buttons) {
                Vector3 center = button.instance.transform.localPosition.normalized * 5 * button.instance.transform.localScale.magnitude;
                float distanceFactor = Vector3.Distance(center, transform.InverseTransformPoint(leftPoint)) / 10;
                distanceFactor = 1 - Mathf.Clamp01(distanceFactor);
                button.instance.transform.localScale = Vector3.Max(button.instance.transform.localScale, Vector3.one * (1 + (distanceFactor * scaleFactor)));

                // Select active button
                if (button.instance.transform.localScale.magnitude > findLeftLargest) {
                    findLeftLargest = button.instance.transform.localScale.magnitude;
                    selectedLeftButton = button;
                }

                button.instance.gameObject.GetComponent<Renderer>().material.color = button.color;
            }
        }
        if (rightActive) {
            foreach (CircleButton button in buttons) {
                Vector3 center = button.instance.transform.localPosition.normalized * 5 * button.instance.transform.localScale.magnitude;
                float distanceFactor = Vector3.Distance(center, transform.InverseTransformPoint(rightPoint)) / 10;
                distanceFactor = 1 - Mathf.Clamp01(distanceFactor);
                button.instance.transform.localScale = Vector3.Max(button.instance.transform.localScale, Vector3.one * (1 + (distanceFactor * scaleFactor)));

                // Select active button
                if (button.instance.transform.localScale.magnitude > findRightLargest) {
                    findRightLargest = button.instance.transform.localScale.magnitude;
                    selectedRightButton = button;
                }

                button.instance.gameObject.GetComponent<Renderer>().material.color = button.color;
            }
        }

        // Button click
        if (selectedLeftButton != null) {
            selectedLeftButton.instance.gameObject.GetComponent<Renderer>().material.color = selectedLeftButton.color + new Color(0.2f, 0.2f, 0.2f);
            if (gripAction.GetStateDown(SteamVR_Input_Sources.LeftHand) == true) {
                selectedLeftButton.callback();
            }
        }

        if (selectedRightButton != null) {
            selectedRightButton.instance.gameObject.GetComponent<Renderer>().material.color = selectedRightButton.color + new Color(0.2f, 0.2f, 0.2f);
            if (gripAction.GetStateDown(SteamVR_Input_Sources.RightHand) == true) {
                selectedRightButton.callback();
            }
        }

    }

    // ----
    public void AddButton(string label, Color color, Action callback, int number = -1)
    {
        buttons.Add(new CircleButton { label = label, color = color, callback = callback, number = number });
    }

    public void Close()
    {
        foreach (Transform child in transform){
            Destroy(child.gameObject);
        }
        buttons.Clear();
        isBuild = false;
    }

    public void ReBuild(Type type)
    {
        transform.rotation = Quaternion.identity;
        // Remove all child elements
        foreach (Transform child in transform) Destroy(child.gameObject);

        this.type = type;
        float totalAngle = 180;
        int amount = buttons.Count - 1;

        if (type == Type.Circle) {
            totalAngle = 360;
            amount = buttons.Count;
        }

        transform.localScale = Vector3.one * size;
        
        int index = 0;
        foreach(CircleButton button in buttons) {
            float angle = -((1f / amount) * index * (totalAngle * Mathf.Deg2Rad));

            bool flip;
            if (type == Type.Circle) {
                flip = (index > amount * 0.5f);
            } else if (type == Type.HalfCircleLeft) {
                flip = true;
            } else {
                flip = false;
            }

            GameObject clone = new GameObject("Button-"+index);
            clone.transform.parent = transform;
            
            if (type == Type.HalfCircleLeft) {
                clone.transform.localPosition = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)); ;
                clone.transform.rotation = Quaternion.Euler(0, 0, -(1f / amount) * index * totalAngle);
            } else {
                clone.transform.localPosition = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)); ;
                clone.transform.rotation = Quaternion.Euler(0, 0, (1f / amount) * index * totalAngle);
            }

            clone.transform.localScale = Vector3.one;
            button.instance = clone;

            // Text object
            GameObject textObject = new GameObject(button.label);
            textObject.transform.parent = clone.transform;
            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.text = button.label;
            text.fontSizeMax = 10;
            text.fontSizeMin = 4;
            text.enableAutoSizing = true;
            text.enableCulling = true;
            if (flip) {
                text.alignment = TextAlignmentOptions.Left;
            } else {
                text.alignment = TextAlignmentOptions.Right;
            }

            RectTransform textTransform = (RectTransform)textObject.transform;
            textTransform.pivot = new Vector2(flip ? 1 : 0, 0.5f);
            textTransform.sizeDelta = new Vector2(7f, 2f);
            textTransform.localScale = new Vector3(1, flip ? -1 : 1, 1);
            textTransform.localPosition = new Vector3(0, 2f, 0.1f);
            textTransform.localRotation = Quaternion.Euler(0, flip ? 0 : 180, flip ? -90 : 90);

            // Number object
            if(button.number != -1) {
                GameObject numberObject = new GameObject(button.number.ToString());
                numberObject.transform.parent = clone.transform;
                TextMeshPro num = numberObject.AddComponent<TextMeshPro>();
                num.text = button.number.ToString();
                num.fontSizeMax = 10;
                num.fontSizeMin = 4;
                num.enableAutoSizing = true;
                num.alignment = TextAlignmentOptions.Center;

                RectTransform numberTransform = (RectTransform)numberObject.transform;
                numberTransform.pivot = new Vector2(flip ? 1 : 0, 0.5f);
                numberTransform.sizeDelta = new Vector2(2.6f, 3f);
                numberTransform.localScale = new Vector3(1, flip ? -1 : 1, 1);
                numberTransform.localPosition = new Vector3(0, 10.3f, 0.1f);
                numberTransform.localRotation = Quaternion.Euler(0, flip ? 0 : 180, flip ? -90 : 90);
            }

            MeshRenderer render = clone.AddComponent<MeshRenderer>();
            render.material = baseMaterial;
            render.material.color = button.color;

            MeshFilter mesh = clone.AddComponent<MeshFilter>();
            float slice = (1f / amount) * (totalAngle * Mathf.Deg2Rad) * 0.5f;
            mesh.mesh = GenerateMesh(slice, button.number != -1);
            index++;

            MeshCollider collider = clone.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh.mesh;
            collider.convex = true;
        }
        isBuild = true;
    }

    private Mesh GenerateMesh(float angle, bool extended)
    {
        Vector2 normalA = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        Vector2 normalB = new Vector2(Mathf.Sin(-angle), Mathf.Cos(-angle));

        Mesh mesh = new Mesh();

        if (!extended) {
            Vector3[] vertices = new Vector3[4]
            {
                normalB * 2,
                normalB * 10,
                normalA * 2,
                normalA * 10,
            };
            int[] tris = new int[6]
            {
                0, 2, 1, 2, 3, 1,
            };
            mesh.vertices = vertices;
            mesh.triangles = tris;
        } else {
            Vector3[] xvertices = new Vector3[8]
            {
                normalB * 2,
                normalB * 10,
                normalA * 2,
                normalA * 10,
                normalB * 10.3f,
                normalB * 13.3f,
                normalA * 10.3f,
                normalA * 13.3f,
            };

            int[] xtris = new int[12]
            {
                0, 2, 1, 2, 3, 1,
                4, 6, 5, 6, 7, 5,
            };
            mesh.vertices = xvertices;
            mesh.triangles = xtris;
        }

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }
}
