using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // Drag your Player here
    public Vector3 offset = new Vector3(0, 5, -8); // Adjustable distance
    public float smoothSpeed = 5f;  // How fast the camera catches up
    public float rotationSmoothness = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Position Following
        // Calculate where the camera *wants* to be (relative to player or world)
        // Using TransformPoint makes the offset rotate WITH the player
        Vector3 desiredPosition = target.TransformPoint(offset);

        // Smoothly interpolate to that position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 2. Rotation Following
        // Make the camera look at the player + slightly up
        Vector3 lookTarget = target.position + Vector3.up * 1.5f;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);

        // Smoothly rotate
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
    }
}