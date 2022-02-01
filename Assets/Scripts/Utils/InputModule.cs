using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class InputModule : BaseInputModule
{
    public Camera Camera; // camera to use for raycast
    public SteamVR_Action_Boolean pinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
    public SteamVR_Input_Sources inputSource;

    private PointerEventData Data = null;
    private GameObject CurrentSelectedObject;
    protected override void Awake()
    {
        base.Awake();

        Data = new PointerEventData(eventSystem);
    }

    public override void Process()
    {
        Data.Reset();
        Data.position = new Vector2(Camera.pixelWidth / 2, Camera.pixelHeight / 2);

        eventSystem.RaycastAll(Data, m_RaycastResultCache);
        Data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        CurrentSelectedObject = Data.pointerCurrentRaycast.gameObject;

        m_RaycastResultCache.Clear();

        HandlePointerExitAndEnter(Data, CurrentSelectedObject);

        if(pinchAction.GetStateDown(inputSource))
        {
            processPress();
        }

        if (pinchAction.GetStateUp(inputSource))
        {
            processRelease();
        }
    }

    private void processPress()
    {
        Data.pointerPressRaycast = Data.pointerCurrentRaycast;

        GameObject pointerPress = ExecuteEvents.ExecuteHierarchy(CurrentSelectedObject, Data, ExecuteEvents.pointerDownHandler);
        if(pointerPress == null)
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
        if(Data.pointerPress = pointerRelease)
        {
            ExecuteEvents.Execute(Data.pointerPress, Data, ExecuteEvents.pointerClickHandler);
        }

        eventSystem.SetSelectedGameObject(null);

        Data.pressPosition = Vector2.zero;
        Data.pointerPress = null;
        Data.rawPointerPress = null;
    }
}
