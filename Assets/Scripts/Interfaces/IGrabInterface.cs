using UnityEngine;

public interface IGrabInterface
{
    void ControllerEnter();
    void ControllerExit();
    void ControllerGrabBegin(GameObject parent);
    void ControllerGrabEnd();
}