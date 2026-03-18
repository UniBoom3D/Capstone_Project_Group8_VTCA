using UnityEngine;

public class CameraFollowOverMap : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Orbit Settings")]
    public float distance = 8f;
    public float pivotHeight = 1.5f;
    public float smoothSpeed = 5f;
    public float rotationSmoothness = 5f;

    [Header("Fixed Angle")]
    [SerializeField] private float fixedAngle = 20f; // Góc nhìn cố định từ trên xuống

    void LateUpdate()
    {
        if (target == null) return;

        // Tính toán vị trí mong muốn dựa trên góc cố định
        Vector3 localOrbitOffset = Quaternion.Euler(fixedAngle, 0, 0) * (Vector3.back * distance);
        localOrbitOffset += Vector3.up * pivotHeight;

        Vector3 desiredPosition = target.TransformPoint(localOrbitOffset);

        // Di chuyển mượt mà
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Nhìn vào mục tiêu (vị trí đầu nhân vật)
        Vector3 lookTarget = target.position + Vector3.up * pivotHeight;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
    }
}