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

    Vector3 lefToRight; // In worldspace
    Vector3 leftToCenter;
    float handleDistance;
    Vector3 initialScale;
    Quaternion initialRotation;

    void StartInteraction()
    {
        Vector3 center = bsphere.transform.position;
        Vector3 left = leftControler.transform.position;
        Vector3 right = rightControler.transform.position;
        lefToRight = right - left;
        leftToCenter = center - left;
        handleDistance = Vector3.Distance(left, right);
        initialScale = transform.localScale;
        initialRotation = transform.rotation;
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

        Quaternion rotation = Quaternion.FromToRotation(lefToRight, newLefToRight);
        Vector3 center = (left + ((rotation * leftToCenter)*sizeFactor));

        transform.position = center + (transform.position - bsphere.transform.position);
        transform.rotation = rotation * initialRotation;
        transform.localScale = initialScale * sizeFactor;

        // Reset the initial values of the rotation at 90 degree thresholds.
        if(rotation.eulerAngles.x > 90.0f && rotation.eulerAngles.x < 270.0f ||
            rotation.eulerAngles.y > 90.0f && rotation.eulerAngles.y < 270.0f ||
            rotation.eulerAngles.z > 90.0f && rotation.eulerAngles.z < 270.0f)
        {
            lefToRight = right - left;
            bsphere.Update();
            leftToCenter = bsphere.transform.position - left;
            handleDistance = Vector3.Distance(left, right);
            initialScale = transform.localScale;
            initialRotation = transform.rotation;
        }
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
