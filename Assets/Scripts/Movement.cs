using UnityEngine;

public class Movement : MonoBehaviour
{
  public float snapTurnAngle = 20;
  public float movementSpeed = 10;
  public Transform teleportPoint;

  bool teleportLock = false;
  bool leftSnapLock = false;
  bool rightSnapLock = false;

  private void Update()
  {
    HandleMovement();
    HandleSnapRotateLeft();
    HandleSnapRotateRight();
    HandleTeleportAction();
  }

  private void HandleMovement()
  {
    transform.position = transform.position + Camera.main.transform.rotation * (new Vector3(ControlerInput.instance.axisLeft.x, 0, ControlerInput.instance.axisLeft.y) * movementSpeed * Time.deltaTime);
  }

  private void HandleTeleportAction()
  {
    if (ControlerInput.instance.axisRight.y < -0.5f && !teleportLock)
    {
      transform.position = teleportPoint.position;
      ControlerInput.instance.VibrateRight();
      teleportLock = true;
    }
    if (ControlerInput.instance.axisRight.y > -0.3f)
    {
      teleportLock = false;
    }
  }

  private void HandleSnapRotateRight()
  {
    if (ControlerInput.instance.axisRight.x < -0.5f && !rightSnapLock)
    {
      transform.Rotate(0, -snapTurnAngle, 0);
      rightSnapLock = true;
    }
    if (ControlerInput.instance.axisRight.x > -0.3f)
    {
      rightSnapLock = false;
    }
  }

  private void HandleSnapRotateLeft()
  {
    if (ControlerInput.instance.axisRight.x > 0.5f && !leftSnapLock)
    {
      transform.Rotate(0, snapTurnAngle, 0);
      leftSnapLock = true;
    }
    if (ControlerInput.instance.axisRight.x < 0.3f)
    {
      leftSnapLock = false;
    }
  }
}
