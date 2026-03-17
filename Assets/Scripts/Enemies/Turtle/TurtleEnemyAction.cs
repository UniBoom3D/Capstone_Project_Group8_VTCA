using UnityEngine;
using System.Collections;

public class TurtleEnemyAction : MonoBehaviour, ITurnParticipant
{
    // =========================================================
    // 🟢 INTERFACE & MỚI
    // =========================================================
    public string Name => gameObject.name;
    public int HP { get; private set; } = 50;
    public bool IsAlive => HP > 0;
    Transform ITurnParticipant.transform { get => this.transform; set { } }

    [Header("Type Settings")]
    public bool IsTurtleCanon = true; // ✅ True: Bắn xa | False: Cận chiến

    [Header("AI Settings")]
    public float moveSpeed = 5f;
    public float attackRange = 15f;
    public float moveDuration = 1.5f;

    [Header("References")]
    public Transform playerTarget;
    public GameObject projectilePrefab;
    public Transform firePoint; // ✅ Rotation X hiện là -45

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage(int dmg)
    {
        HP -= dmg;
        if (HP <= 0) gameObject.SetActive(false);
    }

    public void TakeTurn()
    {
        StartCoroutine(AI_ExecuteTurn());
    }

    // =========================================================
    // 🧠 CHIẾN THUẬT AI (CORE LOGIC)
    // =========================================================
    private IEnumerator AI_ExecuteTurn()
    {
        // 1. Chuẩn bị (Reset trạng thái sau khi bị trúng đạn)
        PrepareForTurn();

        // 2. Tìm mục tiêu
        if (playerTarget == null) FindTarget();
        if (playerTarget == null) yield break;

        // 3. Xoay về phía mục tiêu
        FaceTarget();
        yield return new WaitForSeconds(0.5f);

        // 4. Di chuyển nếu cần
        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance > attackRange)
        {
            yield return StartCoroutine(MoveTowardsTarget());
        }

        // 5. Tấn công dựa trên loại rùa
        yield return new WaitForSeconds(0.5f);
        if (IsTurtleCanon)
        {
            ExecuteRangedAttack();
        }
        else
        {
            ExecuteMeleeAttack();
        }

        Debug.Log($"🐢 {Name} Turn Complete.");
    }

    // =========================================================
    // 🛠 CÁC HÀM NHIỆM VỤ RIÊNG BIỆT
    // =========================================================

    private void PrepareForTurn()
    {
        // Ép đứng thẳng và triệt tiêu lực văng
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void FindTarget()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTarget = p.transform;
    }

    private void FaceTarget()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private IEnumerator MoveTowardsTarget()
    {
        float timer = 0f;
        while (timer < moveDuration)
        {
            if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange) break;
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void ExecuteRangedAttack()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // ✅ Dùng Rotation của firePoint (đã là -45 độ) thay vì tính toán hướng thủ công
        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        ProjectileEnemy projectile = projObj.GetComponent<ProjectileEnemy>();

        if (projectile != null)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            // Tính toán lực dựa trên khoảng cách
            float calculatedPower = (distance * 1.2f) + 25f;
            projectile.Launch(Mathf.Clamp(calculatedPower, 25f, 100f), this);

            // 🎥 THÊM CAMERA NHÌN THEO (SẼ VIẾT Ở BƯỚC TIẾP THEO)
            NotifyCameraOfAttack(projObj.transform);
        }
    }

    private void ExecuteMeleeAttack()
    {
        // Chỗ này để bạn thêm Animation Trigger cho rùa cận chiến sau này
        Debug.Log("🐢 Melee Bite Attack!");
    }

    private void NotifyCameraOfAttack(Transform projectileTransform)
    {
        CameraFollowPlayer cam = Object.FindFirstObjectByType<CameraFollowPlayer>();
        if (cam != null)
        {
            // Chúng ta sẽ thêm hàm LookAtProjectile cho Enemy trong script Camera
            // cam.SetEnemyProjectileTarget(projectileTransform);
        }
    }
}