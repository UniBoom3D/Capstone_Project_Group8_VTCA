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

    private void Start()
    {
        HP = enemyData.baseHealth;
        ai = GetComponent<EnemyAIController>();
    }

    public void TakeDamage(float damage)
    {
        HP -= damage;

        if (HP <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void TakeTurn()
    {
        StartCoroutine(ai.ExecuteTurn(OnTurnFinished));
    }

    void OnTurnFinished()
    {
        Debug.Log("🐢 Enemy finished turn");

        //BattleHandlerPvE.Instance.EndTurn();
    }
}