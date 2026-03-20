using UnityEngine;
using System.Collections;

public class TurtleEnemyAction : MonoBehaviour, ITurnParticipant
{
    // =========================================================
    // 🟢 INTERFACE & MỚI
    // =========================================================
    public string Name => gameObject.name;
    public float HP { get; private set; } = 50;
    public bool IsAlive => HP > 0;
    Transform ITurnParticipant.transform { get => this.transform; set { } }

<<<<<<< HEAD
    float ITurnParticipant.HP => throw new System.NotImplementedException();

    string ITurnParticipant.Name => throw new System.NotImplementedException();

    bool ITurnParticipant.IsAlive => throw new System.NotImplementedException();

    public void TakeDamage(float damage)
    {
        HP -= damage;
        Debug.Log($"💥 {Name} took {damage} damage! Remaining HP: {HP}");
=======
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
>>>>>>> main
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

<<<<<<< HEAD
    void ITurnParticipant.TakeDamage(float damage)
    {
        throw new System.NotImplementedException();
    }

    void ITurnParticipant.TakeTurn()
    {
        throw new System.NotImplementedException();
    }

    void ITurnParticipant.TakeDamage(int dmg)
    {
        throw new System.NotImplementedException();
    }
=======
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

            StartCoroutine(WaitUntilEnemyActionDone(projObj));
        }
    }

    private void ExecuteMeleeAttack()
    {
        Debug.Log("🐢 Melee Bite Attack!");
        // 1. Chơi Animation cắn (Trigger: "Bite")
        // 2. Gây sát thương trực tiếp cho Player nếu đang ở gần
        if (Vector3.Distance(transform.position, playerTarget.position) < 3f)
        {
            ITurnParticipant victim = playerTarget.GetComponent<ITurnParticipant>();
            victim?.TakeDamage(15);
        }
    }

    private IEnumerator WaitUntilEnemyActionDone(GameObject projectile)
    {
        // Đợi đến khi viên đạn nổ (bị Destroy)
        while (projectile != null)
        {
            yield return null;
        }

        // Chờ thêm 1 giây để người chơi thấy hiệu ứng nổ trên Camera bao quát
        yield return new WaitForSeconds(1f);

        // Báo cho BattleHandler chuyển lượt (tùy thuộc vào cách bạn quản lý State)
        Debug.Log("🐢 Enemy action finished visuals.");
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

>>>>>>> main
}