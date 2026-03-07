using UnityEngine;
using System.Collections;

public class EnemyAIController : MonoBehaviour
{
    [Header("References")]
    public EnemyBaseData enemyData;
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Animation")]
    public Animator animator;

    [Tooltip("Animation State Name")]
    public string moveAnim = "Move";
    public string attackAnim = "Attack";
    public string skillAnim = "Skill";

    [Header("Turn Settings")]
    public float turnDuration = 5f;

    private Transform currentTarget;
    private bool hasAttacked = false;

    // =========================================================
    // MAIN TURN LOGIC
    // =========================================================

    public IEnumerator ExecuteTurn(System.Action onTurnFinished)
    {
        Debug.Log("Enemy AI Turn Start");

        hasAttacked = false;

        FindNearestTarget();

        if (currentTarget == null)
        {
            onTurnFinished?.Invoke();
            yield break;
        }

        FaceTarget();

        float timer = 0f;

        while (timer < turnDuration)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);

            if (dist <= enemyData.attackRange)
            {
                yield return AttackRoutine();
                hasAttacked = true;
                break;
            }

            MoveTowardsTarget();

            timer += Time.deltaTime;

            yield return null;
        }

        StopMoveAnimation();

        Debug.Log("Enemy turn finished");

        onTurnFinished?.Invoke();
    }

    // =========================================================
    // TARGETING
    // =========================================================

    void FindNearestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float minDist = Mathf.Infinity;

        foreach (var p in players)
        {
            float d = Vector3.Distance(transform.position, p.transform.position);

            if (d < minDist)
            {
                minDist = d;
                currentTarget = p.transform;
            }
        }
    }

    // =========================================================
    // ROTATION
    // =========================================================

    void FaceTarget()
    {
        Vector3 dir = (currentTarget.position - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    void MoveTowardsTarget()
    {
        PlayMoveAnimation();

        Vector3 dir = (currentTarget.position - transform.position).normalized;
        dir.y = 0;

        transform.position += dir * enemyData.moveSpeed * Time.deltaTime;

        FaceTarget();
    }

    void PlayMoveAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(moveAnim))
        {
            animator.Play(moveAnim);
        }
    }

    void StopMoveAnimation()
    {
        if (animator != null)
        {
            animator.Play("Idle");
        }
    }

    // =========================================================
    // ATTACK
    // =========================================================

    IEnumerator AttackRoutine()
    {
        if (animator != null && !string.IsNullOrEmpty(attackAnim))
        {
            animator.Play(attackAnim);
        }

        yield return new WaitForSeconds(0.35f);

        SpawnProjectile();

        yield return new WaitForSeconds(0.4f);
    }

    void SpawnProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        Vector3 targetPos = currentTarget.position;

        Vector3 dir = targetPos - firePoint.position;

        float angle = enemyData.projectileArcHeight;
        float rad = angle * Mathf.Deg2Rad;

        Vector3 velocity = new Vector3(
            dir.x,
            Mathf.Tan(rad) * new Vector3(dir.x, 0, dir.z).magnitude,
            dir.z
        ).normalized * enemyData.projectilePower;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody rb = proj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }

        Debug.Log("Enemy attacked");
    }

    // =========================================================
    // SKILL (for future)
    // =========================================================

    public IEnumerator SkillRoutine()
    {
        if (animator != null && !string.IsNullOrEmpty(skillAnim))
        {
            animator.Play(skillAnim);
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("Enemy used skill");
    }
}