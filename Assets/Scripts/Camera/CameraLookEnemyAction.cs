using UnityEngine;
using Unity.Cinemachine;

public class CameraLookEnemyAction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineOrbitalFollow orbitalComponent;

    [Header("Rotation Settings")]
    [SerializeField] private float smoothSpeed = 3f;
    [SerializeField] private int activePriority = 110;
    [SerializeField] private int idlePriority = 0;    // Đảm bảo thấp nhất khi Awake

    private float _targetHorizontalValue;
    private bool _isActionDone = false;

    private void Awake()
    {
        if (vcam == null) vcam = GetComponent<CinemachineCamera>();

        if (orbitalComponent == null && vcam != null)
        {
            orbitalComponent = vcam.GetComponent<CinemachineOrbitalFollow>();
        }

        // Ép Priority về thấp nhất khi khởi tạo
        if (vcam != null) vcam.Priority = idlePriority;
    }

    private void Update()
    {
        if (orbitalComponent != null && vcam != null && vcam.Priority > idlePriority)
        {
            // Chỉ thực hiện xoay mượt khi Camera đang Active
            orbitalComponent.HorizontalAxis.Value = Mathf.Lerp(
                orbitalComponent.HorizontalAxis.Value,
                _targetHorizontalValue,
                Time.deltaTime * smoothSpeed
            );
        }
    }

    public void SetEnemyTarget(TurtleEnemyAction enemy)
    {
        if (enemy == null || vcam == null) return;

        // KIỂM TRA TAG: Chỉ nhận nếu là Enemy
        if (!enemy.CompareTag("Enemy"))
        {
            Debug.LogWarning("Target không có tag 'Enemy', bỏ qua tracking.");
            return;
        }

        Transform targetT = enemy.firePoint != null ? enemy.firePoint : enemy.transform;
        SetupCamera(targetT);

        // Logic góc xoay đặc thù của bạn
        if (enemy.IsTurtleCanon)
        {
            _targetHorizontalValue = 150f;
            if (orbitalComponent != null) orbitalComponent.VerticalAxis.Value = 35f;
        }
        else
        {
            _targetHorizontalValue = 160f;
            float currentVertical = orbitalComponent != null ? orbitalComponent.VerticalAxis.Value : 0;
            if (orbitalComponent != null) orbitalComponent.VerticalAxis.Value = 30f;
        }
    }

    public void SetEnemyTarget(ITurnParticipant target)
    {
        if (target is MonoBehaviour mono && mono.CompareTag("Enemy"))
        {
            SetupCamera(mono.transform);
            _targetHorizontalValue = 180f; // Góc mặc định cho ITurnParticipant
        }
    }

    private void SetupCamera(Transform target)
    {
        vcam.Follow = target;
        vcam.LookAt = target;
        vcam.Priority = activePriority; // Nhảy lên để chiếm quyền hiển thị
        _isActionDone = false;
    }

    // Gọi hàm này khi Enemy kết thúc Action (Bắn xong, di chuyển xong)
    public void MarkActionDone()
    {
        _isActionDone = true;
        if (vcam != null) vcam.Priority = idlePriority; // Trả lại quyền cho Camera khác
        Debug.Log("Enemy Camera Action Done - Priority Reset.");
    }
}