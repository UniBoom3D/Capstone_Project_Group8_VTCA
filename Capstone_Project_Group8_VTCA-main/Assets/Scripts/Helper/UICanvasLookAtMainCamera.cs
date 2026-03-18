using UnityEngine;

public class UICanvasLookAtMainCamera : MonoBehaviour
{
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (_mainCamera != null)
        {
            //Lấy vị trí của chính nó trừ đi vị trí Camera
            Vector3 lookDirection = transform.position - _mainCamera.transform.position;

            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                // Tạo góc xoay từ vector
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
}