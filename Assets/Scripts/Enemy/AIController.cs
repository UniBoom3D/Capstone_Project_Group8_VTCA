using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AIController : MonoBehaviour
{
    [Header("AI Settings")]
    public AIDifficulty difficulty = AIDifficulty.Normal;
    public float reactionTime = 0.5f;          // Thời gian phản ứng
    public float aimAccuracy = 0.85f;          // Độ chính xác ngắm (0-1)
    public float decisionDelay = 1f;           // Thời gian suy nghĩ trước khi hành động

    [Header("Player Control Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 60f;
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float maxChargePower = 100f;
    public float chargeSpeed = 40f;

    [Header("AI Behavior")]
    public Transform[] patrolPoints;           // Điểm tuần tra
    public float detectionRange = 30f;         // Tầm phát hiện mục tiêu
    public float minSafeDistance = 5f;         // Khoảng cách an toàn tối thiểu
    public float maxSafeDistance = 15f;        // Khoảng cách tấn công tối ưu

    [Header("Runtime State")]
    public bool isControllable = false;
    private AIState currentState = AIState.Idle;
    private Transform currentTarget;
    private Vector3 moveDestination;
    private bool isCharging = false;
    private float currentChargePower = 0f;
    private CharacterController controller;
    private int currentPatrolIndex = 0;
    private float stateTimer = 0f;

    // Event callback
    public event Action<Projectile> OnShoot;

    public enum AIDifficulty
    {
        Easy,
        Normal,
        Hard,
        Expert
    }

    private enum AIState
    {
        Idle,
        Patrol,
        MoveToTarget,
        Aiming,
        Charging,
        Shooting,
        Evading
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        ApplyDifficulty();
    }

    private void Update()
    {
        if (!isControllable) return;

        stateTimer += Time.deltaTime;
        UpdateAI();
    }

    // ==========================
    // 🧠 MAIN AI LOGIC
    // ==========================
    private void UpdateAI()
    {
        // Tìm mục tiêu gần nhất
        FindTarget();

        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState();
                break;

            case AIState.Patrol:
                HandlePatrolState();
                break;

            case AIState.MoveToTarget:
                HandleMoveToTargetState();
                break;

            case AIState.Aiming:
                HandleAimingState();
                break;

            case AIState.Charging:
                HandleChargingState();
                break;

            case AIState.Evading:
                HandleEvadingState();
                break;
        }
    }

    // ==========================
    // 🎯 TARGET DETECTION
    // ==========================
    private void FindTarget()
    {
        // Tìm tất cả Player trong tầm
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (var hit in hits)
        {
            // Tìm PlayerController (đội địch)
            var player = hit.GetComponent<PlayerController>();
            if (player != null && player.isControllable)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = hit.transform;
                }
            }
        }

        currentTarget = closestTarget;
    }

    // ==========================
    // 📍 STATE: IDLE
    // ==========================
    private void HandleIdleState()
    {
        if (currentTarget != null)
        {
            ChangeState(AIState.MoveToTarget);
            return;
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            ChangeState(AIState.Patrol);
        }
    }

    // ==========================
    // 🚶 STATE: PATROL
    // ==========================
    private void HandlePatrolState()
    {
        if (currentTarget != null)
        {
            ChangeState(AIState.MoveToTarget);
            return;
        }

        if (patrolPoints.Length == 0) return;

        Vector3 targetPoint = patrolPoints[currentPatrolIndex].position;
        MoveTowards(targetPoint);

        if (Vector3.Distance(transform.position, targetPoint) < 1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            stateTimer = 0;
        }
    }

    // ==========================
    // 🏃 STATE: MOVE TO TARGET
    // ==========================
    private void HandleMoveToTargetState()
    {
        if (currentTarget == null)
        {
            ChangeState(AIState.Idle);
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        // Nếu quá gần → Lùi lại
        if (distanceToTarget < minSafeDistance)
        {
            ChangeState(AIState.Evading);
            return;
        }

        // Nếu trong khoảng tối ưu → Ngắm bắn
        if (distanceToTarget <= maxSafeDistance)
        {
            ChangeState(AIState.Aiming);
            return;
        }

        // Di chuyển lại gần
        MoveTowards(currentTarget.position);
        LookAtTarget(currentTarget.position);
    }

    // ==========================
    // 🎯 STATE: AIMING
    // ==========================
    private void HandleAimingState()
    {
        if (currentTarget == null)
        {
            ChangeState(AIState.Idle);
            return;
        }

        LookAtTarget(currentTarget.position);

        // Mô phỏng thời gian ngắm
        if (stateTimer >= reactionTime)
        {
            ChangeState(AIState.Charging);
        }
    }

    // ==========================
    // ⚡ STATE: CHARGING
    // ==========================
    private void HandleChargingState()
    {
        if (currentTarget == null)
        {
            ChangeState(AIState.Idle);
            return;
        }

        LookAtTarget(currentTarget.position);

        if (!isCharging)
        {
            isCharging = true;
            currentChargePower = 0;
        }

        // Tính lực bắn dựa trên khoảng cách
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        float optimalPower = CalculateOptimalPower(distance);

        currentChargePower += chargeSpeed * Time.deltaTime;
        currentChargePower = Mathf.Clamp(currentChargePower, 0, maxChargePower);

        // Bắn khi đạt lực phù hợp (có sai số tùy difficulty)
        float powerThreshold = optimalPower * UnityEngine.Random.Range(0.9f, 1.1f);
        if (currentChargePower >= powerThreshold)
        {
            FireProjectile();
            isCharging = false;
            currentChargePower = 0;
            ChangeState(AIState.Idle);
        }
    }

    // ==========================
    // 🏃‍♂️ STATE: EVADING (Tránh né)
    // ==========================
    private void HandleEvadingState()
    {
        if (currentTarget == null)
        {
            ChangeState(AIState.Idle);
            return;
        }

        // Lùi lại xa mục tiêu
        Vector3 awayDirection = (transform.position - currentTarget.position).normalized;
        Vector3 evadePosition = transform.position + awayDirection * 5f;

        MoveTowards(evadePosition);
        LookAtTarget(currentTarget.position);

        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance >= minSafeDistance + 2f)
        {
            ChangeState(AIState.Aiming);
        }
    }

    // ==========================
    // 🎮 MOVEMENT & ROTATION
    // ==========================
    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Chỉ di chuyển trên mặt phẳng XZ

        controller.Move(direction * moveSpeed * Time.deltaTime);
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Điều chỉnh góc ngắm (firePoint)
        if (firePoint != null && currentTarget != null)
        {
            Vector3 aimDirection = currentTarget.position - firePoint.position;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.magnitude) * Mathf.Rad2Deg;

            // Thêm sai số dựa trên aimAccuracy
            float errorMargin = (1f - aimAccuracy) * 10f;
            angle += UnityEngine.Random.Range(-errorMargin, errorMargin);

            firePoint.localRotation = Quaternion.Euler(-angle, 0, 0);
        }
    }

    // ==========================
    // 🎯 CALCULATE OPTIMAL POWER
    // ==========================
    private float CalculateOptimalPower(float distance)
    {
        // Công thức đơn giản: lực tỉ lệ với khoảng cách
        float power = Mathf.Lerp(30f, maxChargePower, distance / detectionRange);
        return Mathf.Clamp(power, 30f, maxChargePower);
    }

    // ==========================
    // 💥 FIRE PROJECTILE
    // ==========================
    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Launch(currentChargePower, null); // AI không cần owner reference
            OnShoot?.Invoke(projectile);
        }

        Debug.Log($"🤖 AI fired projectile with power {currentChargePower}");
    }

    // ==========================
    // 🔄 STATE MANAGEMENT
    // ==========================
    private void ChangeState(AIState newState)
    {
        currentState = newState;
        stateTimer = 0;
        Debug.Log($"🤖 AI State: {newState}");
    }

    // ==========================
    // ⚙️ DIFFICULTY SETTINGS
    // ==========================
    private void ApplyDifficulty()
    {
        switch (difficulty)
        {
            case AIDifficulty.Easy:
                reactionTime = 1.5f;
                aimAccuracy = 0.6f;
                decisionDelay = 2f;
                moveSpeed = 3f;
                break;

            case AIDifficulty.Normal:
                reactionTime = 0.8f;
                aimAccuracy = 0.75f;
                decisionDelay = 1f;
                moveSpeed = 5f;
                break;

            case AIDifficulty.Hard:
                reactionTime = 0.4f;
                aimAccuracy = 0.9f;
                decisionDelay = 0.5f;
                moveSpeed = 6f;
                break;

            case AIDifficulty.Expert:
                reactionTime = 0.2f;
                aimAccuracy = 0.95f;
                decisionDelay = 0.2f;
                moveSpeed = 7f;
                break;
        }
    }

    // ==========================
    // 🔒 CONTROL TOGGLING
    // ==========================
    public void EnableControl(bool enable)
    {
        isControllable = enable;
        if (enable)
        {
            ChangeState(AIState.Idle);
        }
        Debug.Log($"🤖 AI control: {(enable ? "ENABLED" : "DISABLED")}");
    }

    // ==========================
    // 🎨 GIZMOS FOR DEBUGGING
    // ==========================
    private void OnDrawGizmosSelected()
    {
        // Vẽ tầm phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vẽ khoảng cách an toàn
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minSafeDistance);

        // Vẽ khoảng cách tối ưu
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxSafeDistance);

        // Vẽ đường đến mục tiêu
        if (currentTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}