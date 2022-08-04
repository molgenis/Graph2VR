using Dweiss;
using UnityEngine;

public class Movement : MonoBehaviour
{
  public float snapTurnAngle = 20;
  public float movementSpeed = 10;
  public Transform teleportPoint;
  public Transform cameraPoint;
  public GameObject teleportTarget;

  bool teleportLock = false;
  bool leftSnapLock = false;
  bool rightSnapLock = false;
  private bool isVive = false;
  private void Start()
  {
    ControllerType.instance.GetControllerName((string name) =>
    {
      if (name == "vive")
      {
        isVive = true;
        Settings.Instance.playerHeight = Settings.Instance.playerHeight - 0.55f;
      }
    });
  }

  private void Update()
  {
    HandleMovement();
    HandleSnapRotateLeft();
    HandleSnapRotateRight();
    HandleTeleportAction();
  }

  private void HandleMovement()
  {
    if (ControlerInput.instance.axisLeft.magnitude > 0.1f)
    {
      transform.position = transform.position + Camera.main.transform.rotation * (new Vector3(ControlerInput.instance.axisLeft.x, 0, ControlerInput.instance.axisLeft.y) * movementSpeed * Time.deltaTime);
    }
  }

  private void HandleTeleportAction()
  {
    if ((isVive && ControlerInput.instance.viveRightTrackpadClicked) || !isVive)
    {
      if ((ControlerInput.instance.axisRight.y < -0.5f || (isVive && ControlerInput.instance.axisRight.y > 0.5f)) && !teleportLock)
      {
        teleportTarget.SetActive(true);
        teleportLock = true;
      }
    }
    if ((ControlerInput.instance.axisRight.y > -0.3f && ControlerInput.instance.axisRight.y < 0.3f) || (isVive && !ControlerInput.instance.viveRightTrackpadClicked))
    {
      if (teleportLock)
      {
        teleportTarget.SetActive(false);
        Vector3 cameraOffset = (transform.position - cameraPoint.position);
        cameraOffset = new Vector3(cameraOffset.x, 0, cameraOffset.z);
        transform.position = teleportPoint.position + cameraOffset + new Vector3(0, Settings.Instance.playerHeight, 0);
      }
      teleportLock = false;
    }
    if (teleportLock)
    {
      teleportTarget.transform.position = teleportPoint.position;
    }
  }

  private void HandleSnapRotateRight()
  {
    if ((isVive && ControlerInput.instance.viveRightTrackpadClicked) || !isVive)
    {
      if (ControlerInput.instance.axisRight.x < -0.5f && !rightSnapLock)
      {
        transform.Rotate(0, -snapTurnAngle, 0);
        rightSnapLock = true;
      }
    }
    if (ControlerInput.instance.axisRight.x > -0.3f || (isVive && !ControlerInput.instance.viveRightTrackpadClicked))
    {
      rightSnapLock = false;
    }
  }

  private void HandleSnapRotateLeft()
  {
    if ((isVive && ControlerInput.instance.viveRightTrackpadClicked) || !isVive)
    {
      if (ControlerInput.instance.axisRight.x > 0.5f && !leftSnapLock)
      {
        transform.Rotate(0, snapTurnAngle, 0);
        leftSnapLock = true;
      }
    }
    if (ControlerInput.instance.axisRight.x < 0.3f || (isVive && !ControlerInput.instance.viveRightTrackpadClicked))
    {
      leftSnapLock = false;
    }
  }
}
