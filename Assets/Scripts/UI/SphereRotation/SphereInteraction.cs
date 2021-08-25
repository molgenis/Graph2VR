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

    // In worldspace
    Vector3 leftToCenter;
    float handleDistance;
    Vector3 initialScale;
    Quaternion initialRotation;

    Quaternion initialLookat;


    void StartInteraction()
    {
        Vector3 center = bsphere.transform.position;
        Vector3 left = leftControler.transform.position;
        Vector3 right = rightControler.transform.position;

        Vector3 initialForward = (leftControler.transform.forward + rightControler.transform.forward) * 0.5f;
        initialLookat = Quaternion.LookRotation(initialForward, (right - left).normalized);

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

        Vector3 forward = (leftControler.transform.forward + rightControler.transform.forward) * 0.5f;
        Quaternion lookat = Quaternion.LookRotation(forward, (right - left).normalized);

        float sizeFactor = Vector3.Distance(left, right) / handleDistance;

        Quaternion rotation = lookat * Quaternion.Inverse(initialLookat);
        Vector3 center = (left + ((rotation * leftToCenter)*sizeFactor));

        transform.position = center + (transform.position - bsphere.transform.position);
        transform.rotation = rotation * initialRotation;
        transform.localScale = initialScale * sizeFactor;
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
