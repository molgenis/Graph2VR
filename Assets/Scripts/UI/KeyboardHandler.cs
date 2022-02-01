using UnityEngine;
using UnityEngine.UI;
using VRKeys;

/*
 * Helper Singleton to open and close the VR keys module
 * Use:
 * KeyboardHandler.instance.Open(InputField) to open a keyboard 
 * KeyboardHandler.instance.Close() to close the vr keyboard
 */

public class KeyboardHandler : MonoBehaviour
{
  public Camera cameraToFollow;
  public Vector3 offset;
  private Keyboard keyboard;
  private InputField inputField;
  private Node node;
  private Edge edge;
  private string originalStringValue; // Use to restore string on cancel
  static public KeyboardHandler instance;


  public void Open(InputField input)
  {
    keyboard.Enable();
    keyboard.SetPlaceholderMessage("Please enter something");

    keyboard.OnUpdate.AddListener(HandleUpdate);
    keyboard.OnSubmit.AddListener(HandleSubmit);
    keyboard.OnCancel.AddListener(HandleCancel);
    node = null;
    inputField = input;
    originalStringValue = input.text;
    UpdateLocation();
  }

  // U?se this to edit a node's label
  public void Open(Node node)
  {
    keyboard.Enable();
    keyboard.SetPlaceholderMessage("Please enter label name");

    keyboard.OnUpdate.AddListener(HandleUpdate);
    keyboard.OnSubmit.AddListener(HandleSubmit);
    keyboard.OnCancel.AddListener(HandleCancel);
    keyboard.SetText(node.label);
    inputField = null;
    this.node = node;
    originalStringValue = node.label;
    UpdateLocation();
  }

  // Use this to edit a edge's label
  public void Open(Edge edge)
  {
    keyboard.Enable();
    keyboard.SetPlaceholderMessage("Please enter label name");

    keyboard.OnUpdate.AddListener(HandleUpdate);
    keyboard.OnSubmit.AddListener(HandleSubmit);
    keyboard.OnCancel.AddListener(HandleCancel);
    keyboard.SetText(edge.variableName);
    inputField = null;
    this.edge = edge;
    originalStringValue = edge.variableName;
    UpdateLocation();
  }

  public void Close()
  {
    keyboard.OnUpdate.RemoveListener(HandleUpdate);
    keyboard.OnSubmit.RemoveListener(HandleSubmit);
    keyboard.OnCancel.RemoveListener(HandleCancel);

    keyboard.Disable();
  }

  private void Awake()
  {
    instance = this;
  }

  private void Start()
  {
    keyboard = GetComponent<Keyboard>();
  }

  private void UpdateLocation()
  {
    Quaternion rotation = Quaternion.Euler(0, cameraToFollow.transform.rotation.eulerAngles.y, 0);
    transform.rotation = rotation;
    transform.position = cameraToFollow.transform.position + (rotation * offset);
  }

  public void HandleUpdate(string text)
  {
    keyboard.HideValidationMessage();
    if (inputField != null) {
      inputField.text = text;
    } else if (node != null) {
      node.SetLabel(text);
    } else if (edge != null) {
      edge.variableName = text;
    }
  }

  public void HandleSubmit(string text)
  {
    keyboard.DisableInput();
    Close();
  }

  public void HandleCancel()
  {
    // Restore the string to what it was when opening the VR keyboard
    if (inputField != null) {
      inputField.text = originalStringValue;
    } else if (node != null) {
      node.SetLabel(originalStringValue);
    } else if (edge != null) {
      edge.variableName = originalStringValue;
    }
    keyboard.SetText(originalStringValue);
    Close();
  }
}
