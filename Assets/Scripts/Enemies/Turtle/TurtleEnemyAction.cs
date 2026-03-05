using UnityEngine;
using System.Collections;

public class TurtleEnemyAction : MonoBehaviour, ITurnParticipant
{
    // =========================================================
    // 🟢 INTERFACE IMPLEMENTATION
    // =========================================================
    public string Name => gameObject.name;
    public float HP { get; private set; } = 50;
    public bool IsAlive => HP > 0;

    public void TakeDamage(float damage)
    {
        HP -= damage;
        Debug.Log($"💥 {Name} took {damage} damage! Remaining HP: {HP}");

        if (HP <= 0)
        {
            Debug.Log($"☠ {Name} died.");
            gameObject.SetActive(false);
        }
    }

    public void TakeTurn()
    {
        StartCoroutine(AI_ExecuteTurn());
    }
    // =========================================================

    [Header("AI Settings")]
    public float moveSpeed = 4f;
    public float attackRange = 15f;
    public float moveDuration = 2f;

    [Header("References")]
    public Transform playerTarget;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private IEnumerator AI_ExecuteTurn()
    {
        Debug.Log("🐢 Turtle Turn Started");

        // =========================================================
        // 0. FIND PLAYER
        // =========================================================
        if (playerTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        if (playerTarget == null)
        {
            Debug.LogError("❌ Player not found!");
            yield break;
        }

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        // =========================================================
        // 1️⃣ MOVE
        // =========================================================
        if (distance > attackRange)
        {
            Debug.Log("🐢 Moving toward player...");

            float timer = 0;

            while (timer < moveDuration)
            {
                Vector3 dir = (playerTarget.position - transform.position).normalized;
                dir.y = 0;

                transform.position += dir * moveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(dir);

                timer += Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.4f);

        // =========================================================
        // 2️⃣ AIM ARC
        // =========================================================
        Vector3 targetPos = playerTarget.position;
        Vector3 firePos = firePoint.position;

        Vector3 direction = targetPos - firePos;

        float horizontalDistance = new Vector3(direction.x, 0, direction.z).magnitude;
        float height = direction.y;

        float gravity = Physics.gravity.magnitude;

        // góc bắn mặc định
        float angle = 45f;

        Debug.Log($"🎯 Aim arc calculated (Angle = {angle})");

        yield return new WaitForSeconds(0.4f);

        // =========================================================
        // 3️⃣ CALCULATE POWER
        // =========================================================
        float power = Mathf.Sqrt(horizontalDistance * gravity);

        power = Mathf.Clamp(power, 20f, 80f);

        Debug.Log($"⚡ Power calculated: {power}");

        yield return new WaitForSeconds(0.3f);

        // =========================================================
        // 4️⃣ FIRE
        // =========================================================
        FireProjectile(targetPos, power, angle);

        yield return new WaitForSeconds(0.5f);

        Debug.Log("🐢 Turn Complete");
    }

    private void FireProjectile(Vector3 target, float power, float angle)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("❌ Missing projectile or firePoint");
            return;
        }

        Vector3 dir = target - firePoint.position;

        float rad = angle * Mathf.Deg2Rad;

        Vector3 velocity = new Vector3(
            dir.x,
            Mathf.Tan(rad) * new Vector3(dir.x, 0, dir.z).magnitude,
            dir.z
        ).normalized * power;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody rb = proj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }

        Debug.Log("🔥 Turtle fired projectile!");
    }
}