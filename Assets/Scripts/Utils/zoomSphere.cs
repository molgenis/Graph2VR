using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class zoomSphere : MonoBehaviour
{
    Vector3 rightPos;
    Vector3 leftPos;
    Quaternion rightOrientation;
    Quaternion leftOriantation;
    Vector3 leftdiffPos;
    Vector3 rightdiffPos;
    Vector3 eulerRotation;

    public GameObject zoomGraph;
    Vector3 startScale;
    Quaternion startRotation;
    Vector3 startDirection;
    float startRightAngle = 0;
    float startLeftAngle = 0;

    bool isZooming = false;
    float zoomStart = 0.0f;

    public SteamVR_Action_Boolean gripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool zoomAction = gripAction.GetState(SteamVR_Input_Sources.LeftHand) == true && gripAction.GetState(SteamVR_Input_Sources.RightHand) == true;
        GetComponent<Renderer>().enabled = false; // disable for now

        gameObject.transform.localPosition = (rightPos + leftPos) * 0.5f;
        float currentZoom = (rightPos - leftPos).magnitude;
        if (Math.Abs(zoomStart) > 0.001f)
        {
            gameObject.transform.localScale = startScale * (currentZoom / zoomStart);
        }
        float rightAngleRotationDiff = (startRightAngle - Quaternion.Angle(Quaternion.identity, rightOrientation)) *0.5f;
        float leftAngleRotationDiff = (startLeftAngle - Quaternion.Angle(Quaternion.identity, leftOriantation)) * 0.5f;
        float rotationDiff = rightAngleRotationDiff + leftAngleRotationDiff;
        rotationDiff = Mathf.Clamp(rotationDiff, -90, 90);
        gameObject.transform.rotation = 
            Quaternion.AngleAxis(rotationDiff, (rightPos - leftPos).normalized)
            * Quaternion.FromToRotation(startDirection, (rightPos - leftPos).normalized)
            * startRotation;
        
        if (isZooming)
        {
            if (!zoomAction) {
                isZooming = false;
                zoomGraph.transform.SetParent(null);
            }
        }
        else
        {
            if (zoomAction)
            {
                // Initial values
                isZooming = true;
                zoomGraph.transform.SetParent(gameObject.transform, true);
                zoomStart = (rightPos - leftPos).magnitude;
                startScale = gameObject.transform.localScale;
                startRotation = gameObject.transform.rotation;
                startDirection = (rightPos - leftPos).normalized;

                startRightAngle = Quaternion.Angle(Quaternion.identity, rightOrientation);
                startLeftAngle = Quaternion.Angle(Quaternion.identity, leftOriantation);
            }
        }
    }

    public void ControllerMoved(SteamVR_Behaviour_Pose pose, SteamVR_Input_Sources source)
    {
        if (source == SteamVR_Input_Sources.LeftHand)
        {
            leftPos = pose.poseAction[source].localPosition;
            leftOriantation = pose.poseAction[source].localRotation;
            leftdiffPos = pose.poseAction[source].localPosition - pose.poseAction[source].lastLocalPosition;

        }
        else if (source == SteamVR_Input_Sources.RightHand)
        {
            rightPos = pose.poseAction[source].localPosition;
            rightOrientation = pose.poseAction[source].localRotation;
            rightdiffPos = pose.poseAction[source].localPosition - pose.poseAction[source].lastLocalPosition;
        }
    }
}
