using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using static Unity.Cinemachine.CinemachineAutoFocus;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Cinemachine Reference")]
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;

    [Header("Tags Config")]
    [SerializeField] private string playerFocusTag = "FocusPointPlayer";
    [SerializeField] private string bulletFocusTag = "FocusPointBulletP";

  

    private CinemachineCamera _vcam;

    [Header("Control Settings")]
    public float continuousSpeed = 30f; 
    public float initialDelay = 0.5f;  

    // Biến tạm để xử lý logic nhấn giữ
    private float _keyHoldingTime = 0f;
    private bool _isHolding = false;

    private bool _isDetached = false;

    private void Awake()
    {
        
        if (orbitalFollow == null) orbitalFollow = GetComponent<CinemachineOrbitalFollow>();

        // Lấy CinemachineCamera component
        _vcam = GetComponent<CinemachineCamera>();
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

    private void ApplyChange(float amount)
    {
        float currentValue = orbitalFollow.VerticalAxis.Value;
        orbitalFollow.VerticalAxis.Value = Mathf.Clamp(currentValue + amount, 0f, 60f);
    }
    public float GetCurrentCameraAngle()
    {
        return orbitalFollow != null ? orbitalFollow.VerticalAxis.Value : 0f;
    }

    /// <summary>
    /// Gán camera nhìn vào Focus Point của Player khi bắt đầu lượt mới.
    /// </summary>
    public void SetTarget(Transform nextPlayer)
    {
        if (_vcam == null) return;

        StopAllCoroutines();

        // Đẩy Priority lên cao để chiếm quyền điều khiển từ Intro Cam
        _vcam.Priority = 100;

        Transform focusPoint = FindFocusPoint(nextPlayer, playerFocusTag);
        _vcam.Follow = focusPoint != null ? focusPoint : nextPlayer;
        _vcam.LookAt = focusPoint != null ? focusPoint : nextPlayer;

        Debug.Log($"🎥 Camera focused on: {nextPlayer.name}");
    }

    /// <summary>
    /// Bám theo viên đạn. Khi đạn nổ, camera sẽ dừng lại tại vị trí đó.
    /// </summary>
    public void SetProjectileTarget(Transform projectileTransform)
    {
        if (_vcam == null || projectileTransform == null) return;

        _isDetached = false; // Reset trạng thái cho viên đạn mới
        Transform bulletFocus = FindFocusPoint(projectileTransform, bulletFocusTag);
        Transform targetToFollow = bulletFocus != null ? bulletFocus : projectileTransform;

        _vcam.Follow = targetToFollow;
        _vcam.LookAt = targetToFollow;

        StartCoroutine(TrackProjectileUntilExplosion(targetToFollow));
    }

    private IEnumerator TrackProjectileUntilExplosion(Transform target)
    {
        Quaternion lastValidRotation = _vcam.transform.rotation;

        // Trong khi viên đạn vẫn còn tồn tại
        while (target != null)
        {
            lastValidRotation = _vcam.transform.rotation;
            yield return null;
        }

        // Khi đạn đã biến mất hoàn toàn (sau OnCollisionEnter)
        _vcam.Follow = null;
        _vcam.LookAt = null;
        _vcam.transform.rotation = lastValidRotation;

        Debug.Log("<color=cyan>🎥 Bullet Destroyed. Camera locked.</color>");
    }

    private Transform FindFocusPoint(Transform parent, string tag)
    {
        // Ưu tiên tìm theo tên "Focus Point"
        Transform byName = parent.Find("Focus Point");
        if (byName != null) return byName;

        // Tìm theo Tag (cho Bullet Focus)
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag(tag)) return t;
        }
        return null;
    }

    public void DetachFollow()
    {
        if (_isDetached) return;

        _isDetached = true;
        _vcam.Follow = null; // Ngừng di chuyển camera theo đạn
        Debug.Log("<color=orange>🎥 Camera: Detached Follow. Waiting for explosion...</color>");
    }

}