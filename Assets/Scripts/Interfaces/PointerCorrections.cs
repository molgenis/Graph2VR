using UnityEngine;

public class PointerCorrections : MonoBehaviour
{
  public Vector3 questOffset = Vector3.zero;
  public Vector3 questRotation = Vector3.zero;

  public Vector3 viveOffset = Vector3.zero;
  public Vector3 viveRotation = Vector3.zero;

  void Start()
  {
    ControllerType.instance.GetControllerName((string name) =>
    {
      if (name == "quest")
      {
        transform.localPosition = questOffset;
        transform.localRotation = Quaternion.Euler(questRotation);

        if (PlayerPrefs.GetInt("isLeftHanded", 0) == 1)
        {
          transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        }

      }
      else if (name == "vive")
      {
        transform.localPosition = viveOffset;
        transform.localRotation = Quaternion.Euler(viveRotation);
      }
    });
  }
}
