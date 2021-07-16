using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SphereInteraction : MonoBehaviour
{
    public SteamVR_Action_Boolean gripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
    public BoundingSphere bsphere;
    public bool isActive = false;

    Transform leftControler;
    Transform rightControler;

    // Worldspace vectors
    Vector3 lefToRight;
    Vector3 leftToCenter;
    public float sphereSize;
    float handleDistance;

    void StartInteraction()
    {
        Vector3 center = bsphere.transform.position;
        Vector3 left = leftControler.transform.position;
        Vector3 right = rightControler.transform.position;
        lefToRight = right - left;
        leftToCenter = center - left;
        handleDistance = Vector3.Distance(left, right);
        sphereSize = bsphere.transform.localScale.magnitude;
    }

    void StopInteraction()
    {

    }

    void UpdateInteraction()
    {
        Vector3 left = leftControler.transform.position;
        Vector3 right = rightControler.transform.position;

        Vector3 newLefToRight = right - left;
        float sizeFactor = Vector3.Distance(left, right) / handleDistance;

        // MAX: please help, what is this (2.525) constant? why do i need it? 
        // Something to do with the initial size of the graph?
        // Maybe to do with relative scale of graph object and the bounding sphere?
        // i dont know, please help.. its driving me mad!
        // - Bigger values will make de graph smaller every time it is used
        // - Smaller values will make de grapgh bigger very time it is used
        float size = ((sphereSize) * sizeFactor) / 2.525f;
        Quaternion rotation = Quaternion.FromToRotation(lefToRight, newLefToRight);
        Vector3 center = (left + ((rotation * leftToCenter)*sizeFactor));

        transform.position = center + (transform.position - bsphere.transform.position);
        transform.rotation = rotation;
        transform.localScale = Vector3.one * size;
    }

    private void Start()
    {
        leftControler = GameObject.FindGameObjectWithTag("LeftControler").transform;
        rightControler = GameObject.FindGameObjectWithTag("RightControler").transform;
    }

    void Update()
    {
        bool zoomAction = gripAction.GetState(SteamVR_Input_Sources.LeftHand) && gripAction.GetState(SteamVR_Input_Sources.RightHand);
        if (!isActive && zoomAction) {
            StartInteraction();
            isActive = true;
        }
        if (isActive && !zoomAction) {
            StopInteraction();
            isActive = false;
        }
        if (isActive) {
            UpdateInteraction();
        }
    }

}
