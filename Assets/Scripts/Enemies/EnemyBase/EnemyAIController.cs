using UnityEngine;
using System.Collections;

public class EnemyAIController : MonoBehaviour
{
    [Header("References")]
    public EnemyBaseData enemyData;
    public Transform firePoint;
    public GameObject projectilePrefab;

    private Transform currentTarget;

    public IEnumerator ExecuteTurn(System.Action onTurnFinished)
    {
        Debug.Log(" Enemy AI Turn");

        // 1. Find target
        FindNearestTarget();

        if (currentTarget == null)
        {
            Debug.LogWarning("No target found");
            onTurnFinished?.Invoke();
            yield break;
        }

        // 2. Face target
        FaceTarget();

        yield return new WaitForSeconds(0.3f);

        // 3. Move to attack range
        yield return MoveToAttackRange();

        yield return new WaitForSeconds(0.4f);

        // 4. Attack
        Attack();

        yield return new WaitForSeconds(0.5f);

        onTurnFinished?.Invoke();
    }

    // =========================================================
    // FIND TARGET
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
    // FACE TARGET
    // =========================================================

    void FaceTarget()
    {
        Vector3 dir = (currentTarget.position - transform.position).normalized;
        dir.y = 0;

        transform.rotation = Quaternion.LookRotation(dir);
    }

    // =========================================================
    // MOVE
    // =========================================================

    IEnumerator MoveToAttackRange()
    {
        float dist = Vector3.Distance(transform.position, currentTarget.position);

        while (dist > enemyData.attackRange)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            dir.y = 0;

            transform.position += dir * enemyData.moveSpeed * Time.deltaTime;

            FaceTarget();

            dist = Vector3.Distance(transform.position, currentTarget.position);

            yield return null;
        }

        Debug.Log("Enemy reached attack range");
    }

    // =========================================================
    // ATTACK
    // =========================================================

    void Attack()
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
}