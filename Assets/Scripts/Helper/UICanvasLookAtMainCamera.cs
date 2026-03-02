using UnityEngine;

public class UICanvasLookAtMainCamera : MonoBehaviour
{
    private Camera _mainCamera;

    void Start()
    {
        // Tìm camera chính (Camera Follow Flayer đọc Main Camera)
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (_mainCamera != null)
        {
            // Quay canvas về phía camera, nhưng chỉ giữ góc X và Y
            Vector3 lookDirection = _mainCamera.transform.position - transform.position;
            lookDirection.y = 0; // Đảm bảo chỉ quay theo chiều ngang (giữ hướng Y ổn định)
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}
