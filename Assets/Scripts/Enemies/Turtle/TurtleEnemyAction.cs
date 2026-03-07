using UnityEngine;
using System.Collections;

public class TurtleEnemyAction : MonoBehaviour, ITurnParticipant
{
    public string Name => gameObject.name;
    public float HP { get; private set; }
    public bool IsAlive => HP > 0;

    [Header("Enemy Data")]
    public EnemyBaseData enemyData;

    private EnemyAIController ai;

    void Awake()
    {
        ai = GetComponent<EnemyAIController>();
    }

    void Start()
    {
        HP = enemyData.baseHealth;

        //TurtlePvEManager.Instance.RegisterTurtle(this);
    }

    // =========================
    // DAMAGE
    // =========================

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        HP -= damage;

        Debug.Log($"{Name} took {damage} damage. HP: {HP}");

        if (HP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{Name} died");
        gameObject.SetActive(false);
    }

    // =========================
    // TURN
    // =========================

    public void TakeTurn()
    {
        if (!IsAlive)
        {
            TurtlePvEManager.Instance.NotifyTurnFinished();
            return;
        }

        StartCoroutine(ai.ExecuteTurn(OnTurnFinished));
    }

    void OnTurnFinished()
    {
        Debug.Log($"🐢 {Name} finished turn");

        TurtlePvEManager.Instance.NotifyTurnFinished();
    }
}