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

    float handlesToCenterAngle = 0;
    float axesAngle = 0;
    float currentScaleFactor = 0;
    float handleDistance = 0;
    float centerDistance = 0;

    Vector3 t1 = Vector3.zero;
    Vector3 t2 = Vector3.zero;
    void StartInteraction()
    {
        Vector3 n1 = leftControler.transform.position - bsphere.transform.position;
        Vector3 n2 = rightControler.transform.position - bsphere.transform.position;
        Vector3 handleCenter = rightControler.transform.position + ((leftControler.transform.position - rightControler.transform.position) / 2f);
        centerDistance = Vector3.Distance(handleCenter, bsphere.transform.position);
        handleDistance = Vector3.Distance(leftControler.transform.position, rightControler.transform.position);
        handlesToCenterAngle = Vector3.Angle(n1, n2);
        
        axesAngle = 0; // TODO: calculete that angle of the (leftControler - rightControler) axes
        currentScaleFactor = bsphere.transform.localScale.magnitude;
    }

    void StopInteraction()
    {

    }

    void UpdateInteraction()
    {
        Vector3 lHandle = leftControler.transform.position;
        Vector3 rHandle = rightControler.transform.position;
        float newHandleDistance = Vector3.Distance(lHandle, rHandle);
        float scaleFactor =  (newHandleDistance / handleDistance);
        float newCenterDistance = centerDistance * scaleFactor;
        Vector3 handleCenter = rightControler.transform.position + ((leftControler.transform.position - rightControler.transform.position) / 2f);
        Debug.DrawLine(handleCenter, lHandle, Color.red);
        Debug.DrawLine(handleCenter, rHandle, Color.green);

        // NOTE: this is not 90 degrees
        Vector3 normal = (leftControler.transform.position - rightControler.transform.position).normalized;
        //normal = Quaternion.Euler(axesAngle, 0, 90) * normal;
        normal = Quaternion.FromToRotation(Vector3.forward, Vector3.right) * normal;
        Debug.DrawLine(handleCenter, handleCenter + (normal * newCenterDistance), Color.yellow);


        Vector3 center = handleCenter + (normal * newCenterDistance);

        Debug.DrawLine(center, transform.position, Color.red);
        Debug.DrawLine(center, bsphere.transform.position, Color.blue);
        transform.position = center + (transform.position - bsphere.transform.position);
        transform.localScale = Vector3.one * scaleFactor;
    }

    private void Start()
    {
        leftControler = GameObject.FindGameObjectWithTag("LeftControler").transform;
        rightControler = GameObject.FindGameObjectWithTag("RightControler").transform;
    }

    void Update()
    {
        Debug.DrawLine(t1, t2, Color.white);

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
