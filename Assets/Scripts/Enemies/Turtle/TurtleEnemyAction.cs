using UnityEngine;

public class TurtleEnemyAction : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTarget;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackRange = 30f;

    /// <summary>
    /// BattleHandlerPvE gán target ngay khi spawn
    /// </summary>
    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }

    // =====================================================
    // TURN ENTRY POINT
    // =====================================================

    /// <summary>
    /// Gọi 1 lần duy nhất mỗi turn của enemy
    /// </summary>
    public void ExecuteTurn()
    {
        if (playerTarget == null)
        {
            Debug.LogWarning("[TurtleEnemyAction] Player target missing.");
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            MoveToPlayer();
        }
    }

    // =====================================================
    // ACTIONS
    // =====================================================

    /// <summary>
    /// Di chuyển về phía player cho tới khi vào tầm
    /// </summary>
    public void MoveToPlayer()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;

        // TODO: giới hạn quãng đường di chuyển theo stamina
        transform.position += direction * moveSpeed;

        Debug.Log("🐢 Turtle moves closer to player.");
    }

    /// <summary>
    /// Tấn công player và kết thúc turn
    /// </summary>
    public void AttackPlayer()
    {
        Debug.Log("🐢 Turtle attacks the player!");

        // TODO: gửi event / gọi CombatResolver để tính damage
        // TODO: trừ stamina khi tấn công (sau)

        EndTurn();
    }

    // =====================================================
    // TURN END
    // =====================================================

    private void EndTurn()
    {
        // BattleHandlerPvE sẽ bắt NotifyActionDone() sau này
        Debug.Log("🐢 Turtle ends its turn.");
    }

    private void OnDestroy()
    {
        // Cleanup nếu cần
    }
}
