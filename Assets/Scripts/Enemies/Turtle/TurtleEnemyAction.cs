using UnityEngine;

// ✅ FIXED: Added ITurnParticipant so the BattleManager can control this enemy
public class TurtleEnemyAction : MonoBehaviour, ITurnParticipant
{
    // =========================================================
    // 🟢 INTERFACE IMPLEMENTATION (Required by BattleManager)
    // =========================================================
    public string Name => gameObject.name;

    // 1. HP: We give the turtle 50 HP
    public int HP { get; private set; } = 50;

    // 2. IsAlive: Checks if HP is > 0
    public bool IsAlive => HP > 0;

    // 3. TakeDamage: Logic to lose HP
    public void TakeDamage(int dmg)
    {
        HP -= dmg;
        Debug.Log($"💥 🐢 {Name} took {dmg} damage! Remaining HP: {HP}");
        if (HP <= 0)
        {
            Debug.Log($"💀 {Name} is defeated!");
            gameObject.SetActive(false); // Hide the turtle when dead
        }
    }

    // 4. Transform Fix: Explicitly implement the interface to handle the 'set' requirement
    Transform ITurnParticipant.transform
    {
        get => this.transform;
        set { /* Unity Transform is read-only, so we do nothing */ }
    }

    // 5. TakeTurn: This is what BattleManager calls automatically
    public void TakeTurn()
    {
        // We simply call your custom logic function
        ExecuteTurn();
    }
    // =========================================================

    [Header("Target")]
    [SerializeField] private Transform playerTarget;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackRange = 30f;

    /// <summary>
    /// BattleHandlerPvE assigns target here
    /// </summary>
    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }

    // =====================================================
    // TURN LOGIC
    // =====================================================

    /// <summary>
    /// Called once per turn via TakeTurn()
    /// </summary>
    public void ExecuteTurn()
    {
        // Auto-find player if target is missing
        if (playerTarget == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
        }

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

    public void MoveToPlayer()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;

        // Simple movement (Teleport/Snap for now)
        transform.position += direction * moveSpeed;

        // Look at player
        transform.LookAt(playerTarget);

        Debug.Log("🐢 Turtle moves closer to player.");
    }

    public void AttackPlayer()
    {
        Debug.Log("🐢 Turtle attacks the player!");
        // Logic to deal damage goes here later
    }

    public void OnDestroy()
    {
        // Cleanup if needed
    }
}