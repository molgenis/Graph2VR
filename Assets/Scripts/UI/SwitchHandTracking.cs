using UnityEngine;
using UnityEngine.InputSystem.XR;

public class SwitchHandTracking : MonoBehaviour
{
  public TrackedPoseDriver normal;
  public TrackedPoseDriver switched;

  void Start()
  {
    UpdateLeftRightHandedInterface();
  }

  // Update is called once per frame
  public void UpdateLeftRightHandedInterface()
  {
    bool isLeftHanded = PlayerPrefs.GetInt("isLeftHanded", 0) == 1;
    if (isLeftHanded)
    {
      normal.enabled = false;
      switched.enabled = true;
    }
    else
    {
      normal.enabled = true;
      switched.enabled = false;
    }
  }
}
