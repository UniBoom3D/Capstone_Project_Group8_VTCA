using Unity.AppUI.Core;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Cinemachine Reference")]
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineCamera _vcam;

    [Header("Control Settings")]
    public float continuousSpeed = 30f;
    public float snapAmount = 1.0f;
    [SerializeField] private string playerFocusTag = "FocusPointPlayer";
    private bool _isHolding = false;
    private float _holdTimer = 0f;


    private void Awake()
    {
        if (orbitalFollow == null) orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        _vcam = GetComponent<CinemachineCamera>();
        
        Debug.Log($"CameraFollowPlayer Awake: {orbitalFollow}, {_vcam}");
    }
    private void Start()
    {
        if (_vcam != null)
        {
            SetCameraPriority(0); // Đảm bảo camera bắt đầu với Priority thấp
        }
        
    }

    private void Update()
    {
        // Đọc input trực tiếp tại đây để tránh tranh chấp với PlayerBattleController
        float direction = 0;
        bool isKeyPressed = false;

        if (Input.GetKey(KeyCode.I))
        {
            direction = 1f;
            isKeyPressed = true;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            direction = -1f;
            isKeyPressed = true;
        }

        if (isKeyPressed)
        {
            // Xử lý Snap khi mới nhấn và Hold khi giữ lâu
            HandleInputLogic(direction);
        }
        else
        {
            _isHolding = false;
            _holdTimer = 0f;
        }
    }
    // Hàm này sẽ được PlayerBattleController gọi mỗi Frame khi nhấn phím
    public void ManualUpdateCameraAngle(float direction, bool isSnap)
    {
        if (orbitalFollow == null) return;

        float amount = isSnap ? snapAmount : continuousSpeed * Time.deltaTime;

        // Cộng dồn vào Vertical Axis
        orbitalFollow.VerticalAxis.Value += direction * amount;

        // Giới hạn góc dựa trên thiết lập Three Ring của bạn (0 đến 60)
        orbitalFollow.VerticalAxis.Value = Mathf.Clamp(orbitalFollow.VerticalAxis.Value, 0f, 60f);
    }

    private void HandleInputLogic(float dir)
    {
        if (!_isHolding)
        {
            // Lần đầu nhấn (Snap)
            ManualUpdateCameraAngle(dir, true);
            _isHolding = true;
            _holdTimer = 0f;
        }
        else
        {
            // Giữ phím (Continuous)
            _holdTimer += Time.deltaTime;
            if (_holdTimer >= 0.2f)
            {
                ManualUpdateCameraAngle(dir, false);
            }
        }
    }
    public void SetCameraPriority(int priority)
    {
        if (_vcam != null) _vcam.Priority = priority;
    }

    public float GetCurrentCameraAngle()
    {
        return orbitalFollow != null ? orbitalFollow.VerticalAxis.Value : 0f;
    }

    public void SetTarget(Transform nextPlayer)
    {
        if (_vcam == null) return;
        Transform focusPoint = FindFocusPoint(nextPlayer, playerFocusTag);
        _vcam.Follow = focusPoint != null ? focusPoint : nextPlayer;
        _vcam.LookAt = focusPoint != null ? focusPoint : nextPlayer;
    }

    private Transform FindFocusPoint(Transform parent, string tag)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag(tag)) return t;
        }
        return null;
    }
}