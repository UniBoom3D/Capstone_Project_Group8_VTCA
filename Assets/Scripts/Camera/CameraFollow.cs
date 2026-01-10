using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;        // Drag your Player here

    [Header("Orbit Settings")]
    public float distance = 8f;     // How far the camera stays from the player
    public float pivotHeight = 1.5f;// Height of the point we look at (e.g., player's head)
    public float smoothSpeed = 5f;
    public float rotationSmoothness = 5f;

    [Header("Angle Control (I / K)")]
    public float rotateSpeed = 60f; // Speed in degrees per second
    public float minAngle = -60f;   // Look from below (Low Angle)
    public float maxAngle = 60f;    // Look from above (High Angle)

    // Internal variable to track current angle
    [SerializeField] private float currentAngle = 20f; // Start at 20 degrees (slightly up)

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Handle Input (I / K) to change Angle
        HandleAngleInput();

        // 2. Calculate Desired Position using Rotation
        //    a. Start with a vector pointing backwards (-distance)
        //    b. Rotate it up/down by our 'currentAngle'
        //    c. Add the 'pivotHeight' so we orbit the head, not the feet
        Vector3 localOrbitOffset = Quaternion.Euler(currentAngle, 0, 0) * (Vector3.back * distance);
        localOrbitOffset += Vector3.up * pivotHeight;

        //    d. Convert this local offset to World Position (relative to Player's rotation)
        Vector3 desiredPosition = target.TransformPoint(localOrbitOffset);

        // 3. Smoothly Move Camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 4. Smoothly Look at Target (Look at the pivot point: Head)
        Vector3 lookTarget = target.position + Vector3.up * pivotHeight;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
    }

    void HandleAngleInput()
    {
        // Press I to Increase Angle (Move Camera Up)
        if (Input.GetKey(KeyCode.I))
        {
            currentAngle += rotateSpeed * Time.deltaTime;
        }

        // Press K to Decrease Angle (Move Camera Down)
        if (Input.GetKey(KeyCode.K))
        {
            currentAngle -= rotateSpeed * Time.deltaTime;
        }

        // Clamp angle between -60 (low) and 60 (high)
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
    }
}