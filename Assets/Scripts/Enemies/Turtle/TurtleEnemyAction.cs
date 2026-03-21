using System.Collections;
using System.Linq;
using UnityEngine;

public class TurtleEnemyAction : MonoBehaviour, ITurnParticipant
{
    public string Name => gameObject.name;
    public int HP { get; private set; } = 50;
    public bool IsAlive => HP > 0;
    public bool IsEnemyDead { get; private set; } = false; // Dùng cho Animation & Tính điểm
    public bool isEnemyActionDone { get; set; } = false;

    Transform ITurnParticipant.transform { get => this.transform; set { } }

    [Header("Type Settings")]
    public bool IsTurtleCanon = true;

    [Header("AI Settings")]
    public float moveSpeed = 5f;
    public float attackRange = 15f;
    public float moveDuration = 1.5f;

    [Header("References")]
    public Transform playerTarget;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        // Rigidbody đã được gỡ bỏ theo yêu cầu
    }

    public void TakeDamage(int dmg)
    {
        if (IsEnemyDead) return;

        HP -= dmg;
        if (HP <= 0)
        {
            Die();           
        }
        else
        {
            animator.SetTrigger("GetHit");
            return;
        }
    }

    private void Die()
    {
        IsEnemyDead = true;
        HP = 0;
        animator.SetTrigger("IsDead");

        // Thông báo cho Manager là con này đã hy sinh để tính điểm/spawn mới
        EnemyActionManager.Instance.OnEnemyRemoved(this);

        // Có thể để object tồn tại một chút để xem animation chết trước khi biến mất
        StartCoroutine(DestroyAfterDelay(2f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public void TakeTurn()
    {
        if (!IsAlive || IsEnemyDead) return;
        isEnemyActionDone = false;
        StartCoroutine(AI_ExecuteTurn());
    }

    private IEnumerator AI_ExecuteTurn()
    {
        if (playerTarget == null) FindTarget();
        if (playerTarget == null) { isEnemyActionDone = true; yield break; }

        FaceTarget();

        // Di chuyển
        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance > attackRange)
        {
            yield return StartCoroutine(MoveTowardsTarget());
        }

        // Tấn công
        if (IsTurtleCanon)
        {
            ExecuteRangedAttack();

            // Đợi một Frame để đạn kịp khởi tạo hoàn toàn
            yield return null;

            // TÌM VIÊN ĐẠN VỪA BẮN (Kiểm tra khoảng cách gần firePoint nhất để tránh nhầm đạn cũ)
            ProjectileEnemy lastProj = null;
            var allProjs = Object.FindObjectsByType<ProjectileEnemy>(FindObjectsSortMode.None);

            // Lấy viên đạn gần firePoint nhất trong bán kính 2m
            lastProj = allProjs.FirstOrDefault(p => Vector3.Distance(p.transform.position, firePoint.position) < 2f);

            if (lastProj != null)
            {
                Debug.Log($"🐢 {Name} is tracking projectile: {lastProj.name}");
                while (lastProj != null)
                {
                    yield return null; // Đợi đến khi đạn nổ/bị hủy
                }
            }
        }
        else
        {
            ExecuteMeleeAttack();
            yield return new WaitForSeconds(1.5f);
        }

        yield return new WaitForSeconds(2f);
        isEnemyActionDone = true;
    }

    private void FindTarget()
    {
        GameObject p = GameObject.FindGameObjectWithTag("MeshPlayer");
        if (p != null) playerTarget = p.transform;
    }

    private void FaceTarget()
    {
        Vector3 dir = (playerTarget.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
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

        // 1. Sinh đạn và lưu lại tham chiếu
        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 2. Kích hoạt Animation
        animator.SetBool("FireAttack", true);

        // 3. Tắt animation sau 1 khoảng thời gian (Invoke hoặc Coroutine phụ)
        StartCoroutine(ResetFireAnimation(1f));
    }

    private IEnumerator ResetFireAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetBool("FireAttack", false);
    }

    private void ExecuteMeleeAttack()
    {
        animator?.SetTrigger("MeleeAttack");
        if (Vector3.Distance(transform.position, playerTarget.position) < 3f)
        {
            playerTarget.GetComponent<ITurnParticipant>()?.TakeDamage(15);
        }
    }
}