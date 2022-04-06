using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputModule : BaseInputModule
{
  public Camera Camera; // camera to use for raycast

  private PointerEventData Data = null;
  private GameObject CurrentSelectedObject;
  protected override void Awake()
  {
    base.Awake();

    Data = new PointerEventData(eventSystem);
  }

  private bool lastTriggerState = false;
  public override void Process()
  {
    Data.Reset();
    Data.position = new Vector2(Camera.pixelWidth / 2, Camera.pixelHeight / 2);

    eventSystem.RaycastAll(Data, m_RaycastResultCache);
    Data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
    CurrentSelectedObject = Data.pointerCurrentRaycast.gameObject;

    m_RaycastResultCache.Clear();

    HandlePointerExitAndEnter(Data, CurrentSelectedObject);

    if (ControlerInput.instance.triggerRight == true && lastTriggerState == false)
    {
      processPress();
      lastTriggerState = ControlerInput.instance.triggerRight;
    }
    else if (ControlerInput.instance.triggerRight == false && lastTriggerState == true)
    {
      processRelease();
      lastTriggerState = ControlerInput.instance.triggerRight;
    }
  }

  private void processPress()
  {
    Data.pointerPressRaycast = Data.pointerCurrentRaycast;

    GameObject pointerPress = ExecuteEvents.ExecuteHierarchy(CurrentSelectedObject, Data, ExecuteEvents.pointerDownHandler);
    if (pointerPress == null)
    {
      pointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(CurrentSelectedObject);
    }

    Data.pressPosition = Data.position;
    Data.pointerPress = pointerPress;
    Data.rawPointerPress = CurrentSelectedObject;
  }

  private void processRelease()
  {
    ExecuteEvents.Execute(Data.pointerPress, Data, ExecuteEvents.pointerUpHandler);

    GameObject pointerRelease = ExecuteEvents.GetEventHandler<IPointerClickHandler>(CurrentSelectedObject);
    if (Data.pointerPress = pointerRelease)
    {
      ExecuteEvents.Execute(Data.pointerPress, Data, ExecuteEvents.pointerClickHandler);
    }

    eventSystem.SetSelectedGameObject(null);

    Data.pressPosition = Vector2.zero;
    Data.pointerPress = null;
    Data.rawPointerPress = null;
  }
}
