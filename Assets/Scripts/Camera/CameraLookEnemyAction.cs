using UnityEngine;
using Unity.Cinemachine;

public class CameraLookEnemyAction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineOrbitalFollow orbitalComponent;

    [Header("Rotation Settings")]
    [SerializeField] private float smoothSpeed = 3f;
    private float _targetHorizontalValue;

    private void Awake()
    {
        if (vcam == null) vcam = GetComponent<CinemachineCamera>();

        // Cách lấy component Orbital chuẩn cho Cinemachine v3
        if (orbitalComponent == null && vcam != null)
        {
            orbitalComponent = vcam.GetComponent<CinemachineOrbitalFollow>();
        }
    }


    private void Update()
    {
        if (orbitalComponent != null)
        {
            // Thực hiện xoay mượt mà đến góc mục tiêu
            orbitalComponent.HorizontalAxis.Value = Mathf.Lerp(
                orbitalComponent.HorizontalAxis.Value,
                _targetHorizontalValue,
                Time.deltaTime * smoothSpeed
            );
        }
    }

    // Đổi tên hàm này để khớp với BattleHandlerPvE
    public void ResetHorizontalBeforeFocus(float startAngle)
    {
        if (orbitalComponent != null)
        {
            orbitalComponent.HorizontalAxis.Value = startAngle;
            _targetHorizontalValue = startAngle;
        }
    }

    public void SetEnemyTarget(TurtleEnemyAction enemy)
    {
        if (enemy == null || vcam == null) return;

        // Ưu tiên nhìn vào FirePoint của rùa
        Transform targetT = enemy.firePoint != null ? enemy.firePoint : enemy.transform;

        vcam.Follow = targetT;
        vcam.LookAt = targetT;

        // Thiết lập góc xoay mục tiêu dựa trên loại rùa
        if (enemy.IsTurtleCanon)
        {
            _targetHorizontalValue = 150f;
            if (orbitalComponent != null) orbitalComponent.VerticalAxis.Value = 35f;
        }
        else
        {
            _targetHorizontalValue = 160f;
            if (orbitalComponent != null) orbitalComponent.VerticalAxis.Value = 30f;
        }
    }
}