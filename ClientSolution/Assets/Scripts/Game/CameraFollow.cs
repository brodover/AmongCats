using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    private void FixedUpdate()
    {
        if (target == null) { return; }
        Vector3 targetPosition =
               target.position + new Vector3(0, 0, -1);
        transform.position = Vector3.SmoothDamp(
               transform.position,
               targetPosition,
               ref velocity,
               smoothTime);
    }
}
