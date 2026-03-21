using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EyeScouterGuide : MonoBehaviour
{
    [Header("Setup")]
    public Transform eyePosition;      // Điểm bắt đầu (Mắt)
    public Transform scouterCenter;    // Điểm hướng tới (Tâm kính/Camera forward)
    public LayerMask collisionMask;

    [Header("Settings")]
    public float maxDistance = 15f;
    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        // Thiết lập tia laser màu đỏ, mảnh
        lr.startWidth = 0.06f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = Color.red;
        lr.useWorldSpace = true;
        lr.enabled = false;
    }

    // Hàm điều khiển hiển thị từ bên ngoài
    public void SetVisible(bool visible)
    {
        if (lr != null) lr.enabled = visible;
    }

    private void Update()
    {
        if (!lr.enabled || eyePosition == null || scouterCenter == null) return;

        DrawRay();
    }

    private void DrawRay()
    {
        Vector3 startPos = eyePosition.position;
        // Tia luôn đi thẳng từ mắt qua tâm kính (đã xoay theo camera)
        Vector3 direction = (scouterCenter.position - eyePosition.position).normalized;

        lr.SetPosition(0, startPos);

        if (Physics.Raycast(startPos, direction, out RaycastHit hit, maxDistance, collisionMask))
        {
            lr.SetPosition(1, hit.point);
        }
        else
        {
            lr.SetPosition(1, startPos + direction * maxDistance);
        }
    }
}