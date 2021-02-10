using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * Add this to GameObject with a InputField to automagicly open a VR keyboard to input text
 * Need a VRKeys GameObject with a KeyboardHandler MonoBehaviour
 */

public class AutoOpenKeyboard : MonoBehaviour, ISelectHandler
{
    private InputField input;

    public void OnSelect(BaseEventData eventData)
    {
        KeyboardHandler.instance.Open(input);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        KeyboardHandler.instance.Close();
    }

    void Start()
    {
        input = GetComponent<InputField>();
    }
}
