using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabInterface
{
    void ControllerEnter();
    void ControllerExit();
    void ControllerGrabBegin(GameObject parent);
    void ControllerGrabEnd();
}