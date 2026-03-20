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

    private void Awake()
    {
        if (orbitalFollow == null) orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        _vcam = GetComponent<CinemachineCamera>();
        _vcam.Priority = 0;
    }

    // Hàm này sẽ được PlayerBattleController gọi mỗi Frame khi nhấn phím
    public void ManualUpdateCameraAngle(float direction, bool isSnap)
    {
        Debug.Log($"Camera received call: ");
        if (orbitalFollow == null) return;

        if (isSnap)
        {
            // Nhảy 1 độ mỗi lần nhấn
            orbitalFollow.VerticalAxis.Value += direction * 1.0f;
        }
        else
        {
            // Quay mượt 30 độ mỗi giây khi giữ
            orbitalFollow.VerticalAxis.Value += direction * 30f * Time.deltaTime;
        }    
        float speed = isSnap ? 1.0f : 30f * Time.deltaTime;
        // Giới hạn góc từ 0 đến 60
        orbitalFollow.VerticalAxis.Value = Mathf.Clamp(orbitalFollow.VerticalAxis.Value, 0f, 60f);
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