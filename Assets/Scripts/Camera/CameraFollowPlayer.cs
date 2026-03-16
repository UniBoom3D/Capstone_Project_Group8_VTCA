using UnityEngine;
using Unity.Cinemachine;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Cinemachine Reference")]
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;

    [Header("Control Settings")]
    public float continuousSpeed = 30f; // Tốc độ khi nhấn giữ
    public float initialDelay = 0.5f;   // Thời gian chờ để xác nhận nhấn giữ

    // Biến tạm để xử lý logic nhấn giữ
    private float _keyHoldingTime = 0f;
    private bool _isHolding = false;

    private void Awake()
    {
        if (orbitalFollow == null)
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        if (orbitalFollow == null) return;

        HandleSmartInput(KeyCode.I, -1f); // Nhìn lên
        HandleSmartInput(KeyCode.K, 1f);  // Nhìn xuống
    }

    private void HandleSmartInput(KeyCode key, float direction)
    {
        // 1. Khi vừa nhấn phím xuống (Single Press)
        if (Input.GetKeyDown(key))
        {
            ApplyChange(direction * 1f); // Nhảy 1 đơn vị ngay lập tức
            _keyHoldingTime = 0f;
            _isHolding = true;
        }

        // 2. Trong khi đang giữ phím
        if (Input.GetKey(key) && _isHolding)
        {
            _keyHoldingTime += Time.deltaTime;

            // Nếu giữ lâu hơn 0.5s thì bắt đầu chạy liên tục
            if (_keyHoldingTime >= initialDelay)
            {
                ApplyChange(direction * continuousSpeed * Time.deltaTime);
            }
        }

        // 3. Khi thả phím
        if (Input.GetKeyUp(key))
        {
            _isHolding = false;
            _keyHoldingTime = 0f;
        }
    }

    public float GetCurrentCameraAngle()
    {
        return orbitalFollow != null ? orbitalFollow.VerticalAxis.Value : 0f;
    }

    private void ApplyChange(float amount)
    {
        float currentValue = orbitalFollow.VerticalAxis.Value;
        orbitalFollow.VerticalAxis.Value = Mathf.Clamp(currentValue + amount, 0f, 60f);
    }

    public void SetTarget(Transform newPlayerTransform)
    {
        if (orbitalFollow == null) return;
        if (!orbitalFollow.gameObject.TryGetComponent<CinemachineCamera>(out var vcam)) return;

        // Tìm Focus Point (Lớp 1: Theo tên | Lớp 2: Theo Tag)
        Transform focusPoint = FindFocusPoint(newPlayerTransform);

        if (focusPoint != null)
        {
            // Cập nhật cả Tracking Target (Follow) và Look At Target (LookAt) vào Focus Point
            // Điều này giúp camera xoay quanh và nhìn thẳng vào điểm này
            vcam.Follow = focusPoint;
            vcam.LookAt = focusPoint;

            Debug.Log($"<color=green>🎥 Camera Linked: {focusPoint.name} set as Follow & LookAt</color>");
        }
        else
        {
            // Fallback nếu không có Focus Point
            vcam.Follow = newPlayerTransform;
            vcam.LookAt = newPlayerTransform;
            Debug.LogWarning($"⚠️ Fallback: Không tìm thấy Focus Point, gán vào gốc {newPlayerTransform.name}");
        }
    }

    private Transform FindFocusPoint(Transform parent)
    {
        // Lớp 1: Tìm theo tên chính xác
        Transform byName = parent.Find("Focus Point");
        if (byName != null) return byName;

        // Lớp 2: Tìm trong các con theo Tag "FocusPointPlayer"
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("FocusPointPlayer")) return t;
        }

        return null;
    }
}