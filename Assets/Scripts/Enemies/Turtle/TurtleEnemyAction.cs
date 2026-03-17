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

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        // triệt tiêu vận tốc cũ để nó không bị trượt
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        // 2. Face the Player
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(directionToPlayer);

        yield return new WaitForSeconds(0.5f); // Đợi ổn định sau khi xoay

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
            // 🚀 TÍNH TOÁN HƯỚNG BẮN (Aming)
            // Tạo một hướng bắn hơi chếch lên trên để đạn bay cầu vồng (tránh chạm đất ngay lập tức)
            Vector3 lowAngleDir = (playerTarget.position - firePoint.position).normalized;
            Vector3 highAngleDir = (lowAngleDir + Vector3.up * 0.5f).normalized; // Chếch lên 0.5 đơn vị

            GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(highAngleDir));
            ProjectileEnemy projectile = projObj.GetComponent<ProjectileEnemy>();

            if (projectile != null)
            {
                float distance = Vector3.Distance(transform.position, playerTarget.position);

                // 🚀 TÍNH TOÁN LỰC (Power)
                // Lực tỷ lệ thuận với khoảng cách. 
                // Nếu khoảng cách là 10m, lực khoảng 35. Nếu 20m, lực khoảng 50.
                float calculatedPower = (distance * 1.2f) + 25f;

                // Giới hạn lực để không bắn quá yếu hoặc quá mạnh
                projectile.Launch(Mathf.Clamp(calculatedPower, 25f, 100f), this);

                Debug.Log($"🔥 AI Fired with Power: {calculatedPower:F1} at distance: {distance:F1}");
            }
        }
    }
}