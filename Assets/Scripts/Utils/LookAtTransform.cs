using UnityEngine;

public class LookAtTransform : MonoBehaviour
{
    public Transform lookAt = null;
    public bool flipDirection = false;
    public Vector3 normal;

    private void Start()
    {
        if (lookAt == null) lookAt = Camera.main.transform;
    }

    void Update()
    {
        normal = (transform.position - lookAt.position).normalized;
        if (normal.sqrMagnitude > 0) {
            transform.rotation = Quaternion.LookRotation(normal * (flipDirection ? -1 : 1));
        }
    }
}
