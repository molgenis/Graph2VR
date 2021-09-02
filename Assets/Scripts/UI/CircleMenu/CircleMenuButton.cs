using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CircleMenuButton : MonoBehaviour
{
    private SteamVR_Action_Boolean pinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");

    CircleMenu menu;
    float initialAngle;
    readonly float falloff = 30f;
    GameObject controler;
    CircleMenu.CircleButton button;

    public void Set(CircleMenu menu, float initialAngle, CircleMenu.CircleButton button)
    {
        this.menu = menu;
        this.initialAngle = initialAngle;
        this.button = button;

        controler = GameObject.FindGameObjectWithTag("RightControler");
        pinchAction[SteamVR_Input_Sources.RightHand].onChange += SteamVR_Behaviour_Pinch_OnChange;
    }

    void Update()
    {
        // Auto rotate based on slider
        float angle = menu.GetMenuAngle() + initialAngle;
        transform.localPosition = new Vector2(Mathf.Sin(-angle * Mathf.Deg2Rad), Mathf.Cos(-angle * Mathf.Deg2Rad)); ;
        transform.localRotation = Quaternion.Euler(0, 0, angle);

        // Auto scale down if angle is outside of 0 ... 180
        float scaleFactor = 0;
        if (angle < 0) {
            scaleFactor = 1-(Mathf.Clamp(-angle, 0, falloff) / falloff);
        }
        if (angle >= 0 && angle <= 180) {
            scaleFactor = 1;
        }
        if (angle > 180) {
            scaleFactor = 1-(Mathf.Clamp(angle-180, 0, falloff) / falloff);
        }

        transform.localScale = Vector3.one * scaleFactor;

        // Collider ray check
        RaycastHit hit;
        Collider collider = GetComponent<Collider>();
        if (collider != null) {
            // TODO: move raycast to controler for nice speedup!
            if (collider.Raycast(new Ray(controler.transform.position, controler.transform.forward), out hit, 2f)) {
                // Someone is pointing at us
                gameObject.GetComponent<Renderer>().material.color = button.color + new Color(0.2f, 0.2f, 0.2f); ;

                // Someone is clicking at us
                if (actionPressed) {
                    button.callback();
                    actionPressed = false;
                }
            } else {
                gameObject.GetComponent<Renderer>().material.color = button.color;
            }
        }
    }

    bool actionPressed = false;
    private void SteamVR_Behaviour_Pinch_OnChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
    {
        actionPressed = newState;
    }
}
