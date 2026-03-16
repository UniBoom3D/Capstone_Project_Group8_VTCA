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

        Transform focusPoint = FindFocusPoint(nextPlayer, playerFocusTag);

        // Trong v3.x, gán trực tiếp vào Follow và LookAt của CinemachineCamera
        _vcam.Follow = focusPoint != null ? focusPoint : nextPlayer;
        _vcam.LookAt = focusPoint != null ? focusPoint : nextPlayer;
    }

    /// <summary>
    /// Bám theo viên đạn. Khi đạn nổ, camera sẽ dừng lại tại vị trí đó.
    /// </summary>
    public void SetProjectileTarget(Transform projectileTransform)
    {
        if (_vcam == null || projectileTransform == null) return;

        // Tìm điểm Focus bên trong viên đạn
        Transform bulletFocus = FindFocusPoint(projectileTransform, bulletFocusTag);
        Transform targetToFollow = bulletFocus != null ? bulletFocus : projectileTransform;

        _vcam.Follow = targetToFollow;
        _vcam.LookAt = targetToFollow;

        StartCoroutine(TrackProjectileUntilExplosion(targetToFollow));
    }

    private IEnumerator TrackProjectileUntilExplosion(Transform target)
    {
        // Khi viên đạn/focus point còn tồn tại
        while (target != null)
        {
            yield return null;
        }

        // Đạn nổ: Gán null để Camera đứng im tại chỗ
        if (_vcam != null)
        {
            _vcam.Follow = null;
            _vcam.LookAt = null;
        }
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
   
}