using UnityEngine;
using Unity.Cinemachine;

public class CameraFollowProjectile : MonoBehaviour
{
    private CinemachineCamera _vcam;
    private bool _isExploded = false;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineCamera>();
        // Khởi tạo ở mức thấp, chỉ cao lên khi được bắn
        _vcam.Priority = 0;
    }

    public void ActivateCamera(Transform projectileTransform)
    {
        if (_vcam == null) return;

        _vcam.Follow = projectileTransform;
        _vcam.LookAt = projectileTransform;
        _vcam.Priority = 200; // Cao hơn hẳn PlayerCam (100) và IntroCam
        _isExploded = false;
    }

    public void OnProjectileHit()
    {
        if (_isExploded) return;
        _isExploded = true;

        // Đảm bảo Follow đã ngắt
        if (_vcam != null) _vcam.Follow = null;

        // Đợi 2 giây xem hiệu ứng nổ rồi mới trả Priority về 0
        Invoke(nameof(ResetCamera), 2.0f);
    }
    public void DetachFollow()
    {
        if (_vcam != null) _vcam.Follow = null;
        // Camera dừng di chuyển nhưng LookAt vẫn còn để nhìn về phía đạn
    }
    private void ResetCamera()
    {
        _vcam.Priority = 0;
        _vcam.LookAt = null;
    }
}