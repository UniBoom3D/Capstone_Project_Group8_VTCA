using UnityEngine;
using System.Collections;

public class TurtleEnemyAction : MonoBehaviour, ITurnParticipant
{
    // =========================================================
    // 🟢 INTERFACE IMPLEMENTATION
    // =========================================================
    public string Name => gameObject.name;
    public int HP { get; private set; } = 50;
    public bool IsAlive => HP > 0;
    Transform ITurnParticipant.transform { get => this.transform; set { } }

    public void TakeDamage(int dmg)
    {
        HP -= dmg;
        Debug.Log($"💥 {Name} took {dmg} damage! Remaining HP: {HP}");
        if (HP <= 0) gameObject.SetActive(false);
    }

    public void TakeTurn()
    {
        StartCoroutine(AI_ExecuteTurn());
    }
    // =========================================================

    [Header("AI Settings")]
    public float moveSpeed = 5f;
    public float attackRange = 15f; // ✅ Reduced to 15 so it walks first!
    public float moveDuration = 1.5f;

    [Header("References")]
    public Transform playerTarget;      // ✅ Manual slot to ensure we find player
    public GameObject projectilePrefab;
    public Transform firePoint;

    private IEnumerator AI_ExecuteTurn()
    {
        Debug.Log("🐢 Turtle Turn Started...");

        // 1. Find Player (Auto or Manual)
        if (playerTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        // 🚨 ERROR CHECK: If we still have no target, complain loudly!
        if (playerTarget == null)
        {
            Debug.LogError("❌ TURTLE ERROR: Cannot find Player! Make sure Player has tag 'Player' or drag it into the Inspector slot.");
            yield break;
        }

        // 2. Face the Player
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(directionToPlayer);

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        Debug.Log($"🐢 Distance to Player: {distance:F1} (Range: {attackRange})");

        // 3. MOVE (Only if far away)
        if (distance > attackRange)
        {
            Debug.Log("🐢 Moving closer...");
            float timer = 0f;
            while (timer < moveDuration)
            {
                if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange) break;
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
                timer += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            Debug.Log("🐢 Already in range, skipping movement.");
        }

        // 4. ATTACK
        yield return new WaitForSeconds(0.5f); // Take aim
        AttackPlayer();

        Debug.Log("🐢 Turn Complete.");
    }

    private void AttackPlayer()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Debug.Log("🔥 TURTLE FIRE!");

            // Aim at player + slightly up
            Vector3 aimDir = (playerTarget.position - firePoint.position).normalized;

            GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(aimDir));
            Projectile projectile = projObj.GetComponent<Projectile>();

            if (projectile != null)
            {
                float power = Vector3.Distance(transform.position, playerTarget.position) * 1.5f;
                projectile.Launch(Mathf.Clamp(power, 20f, 80f), this);
            }
        }
        else
        {
            Debug.LogError("❌ Turtle missing Projectile Prefab or FirePoint!");
        }
    }
}