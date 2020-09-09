using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class zoomSphere : MonoBehaviour
{
    Vector3 rightPos;
    Vector3 leftPos;
    Vector3 leftdiffPos;
    Vector3 rightdiffPos;
    Vector3 eulerRotation;

    public GameObject zoomGraph;
    Vector3 startScale;
    Quaternion startRotation;
    Vector3 startDirection;

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
        gameObject.transform.localPosition = (rightPos + leftPos) * 0.5f;
        bool zoomAction = gripAction.GetState(SteamVR_Input_Sources.LeftHand) == true && gripAction.GetState(SteamVR_Input_Sources.RightHand) == true;
        GetComponent<Renderer>().enabled = zoomAction;
        if(isZooming)
        {
            if (zoomAction)
            {
                float currentZoom = (rightPos - leftPos).magnitude;
                zoomGraph.transform.localScale = startScale*(currentZoom / zoomStart);
                zoomGraph.transform.position = zoomGraph.transform.position + (leftdiffPos + rightdiffPos) * 0.5f;
                zoomGraph.transform.rotation = (Quaternion.FromToRotation(startDirection, (rightPos - leftPos).normalized)) * startRotation;
            }
            else
            {
                isZooming = false;
            }
        }
        else
        {
            if (zoomAction)
            {
                isZooming = true;
                zoomStart = (rightPos - leftPos).magnitude;
                startScale = zoomGraph.transform.localScale;
                startRotation = zoomGraph.transform.rotation;
                startDirection = (rightPos - leftPos).normalized;
            }
        }
    }

    public void ControllerMoved(SteamVR_Behaviour_Pose pose, SteamVR_Input_Sources source)
    {
        if (source == SteamVR_Input_Sources.LeftHand)
        {
            leftPos = pose.poseAction[source].localPosition;
            leftdiffPos = pose.poseAction[source].localPosition - pose.poseAction[source].lastLocalPosition;

        }
        else if (source == SteamVR_Input_Sources.RightHand)
        {
            rightPos = pose.poseAction[source].localPosition;
            rightdiffPos = pose.poseAction[source].localPosition - pose.poseAction[source].lastLocalPosition;
        }
    }
}
