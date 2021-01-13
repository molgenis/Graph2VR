using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;

public class GraphInteract : MonoBehaviour
{
    private SteamVR_Action_Boolean grabAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
    public SteamVR_Input_Sources inputSource;

    private GameObject CurrentHoveredObject = null;
    private GameObject GrabbedObject = null;

    // Start is called before the first frame update
    void Start()
    {
        grabAction[inputSource].onChange += SteamVR_Behaviour_Grab_OnChange;
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] overlapping = Physics.OverlapSphere(transform.position, 0.03f);
        GameObject closestObject = null;
        foreach(Collider col in overlapping)
        {
            GameObject colliderAsGrab = null;
            if (col.gameObject.GetComponent<IGrabInterface>() != null)
            {
                colliderAsGrab = col.gameObject;
            }
            if (colliderAsGrab != null)
            {
                if (closestObject != null)
                {
                    if (Vector3.SqrMagnitude(transform.position - col.gameObject.transform.position) < Vector3.SqrMagnitude(transform.position - closestObject.transform.position))
                    {
                        closestObject = colliderAsGrab;
                    }
                }
                else
                {
                    closestObject = colliderAsGrab;
                }
            }
        }

        HandleHoveredObject(closestObject);
    }

    void HandleHoveredObject(GameObject newHoveredObject)
    {
        IGrabInterface newGrabAble = null;
        if(newHoveredObject)
        {
            newGrabAble = newHoveredObject.GetComponent<IGrabInterface>();
        }
        if (newGrabAble == null)
        {
            if(CurrentHoveredObject != null)
            {
                CurrentHoveredObject.GetComponent<IGrabInterface>().ControllerExit();
                CurrentHoveredObject = null;
            }
        }
        else
        {
            if(newHoveredObject != CurrentHoveredObject)
            {
                if (CurrentHoveredObject)
                {
                    CurrentHoveredObject.GetComponent<IGrabInterface>().ControllerExit();
                }
                newGrabAble.ControllerEnter();
                CurrentHoveredObject = newHoveredObject;
            }
        }
    }

    private void SteamVR_Behaviour_Grab_OnChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
    {
        if(CurrentHoveredObject != null)
        {
            if(newState)
            {
                CurrentHoveredObject.GetComponent<IGrabInterface>().ControllerGrabBegin(this.gameObject);
                GrabbedObject = CurrentHoveredObject;
            }
            else
            {
                CurrentHoveredObject.GetComponent<IGrabInterface>().ControllerGrabEnd();
                GrabbedObject = null;
            }
        }
    }
}