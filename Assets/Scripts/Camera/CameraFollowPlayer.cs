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
        if (orbitalFollow != null)
        {
            return orbitalFollow.VerticalAxis.Value;
        }
        return 0f;
    }

    private void ApplyChange(float amount)
    {
        float currentValue = orbitalFollow.VerticalAxis.Value;
        orbitalFollow.VerticalAxis.Value = Mathf.Clamp(currentValue + amount, 0f, 60f);
    }

    public void SetTarget(Transform newTarget)
    {
        if (orbitalFollow != null && orbitalFollow.gameObject.TryGetComponent<CinemachineCamera>(out var vcam))
        {
            vcam.Follow = newTarget;
            vcam.LookAt = newTarget;
        }
    }
}