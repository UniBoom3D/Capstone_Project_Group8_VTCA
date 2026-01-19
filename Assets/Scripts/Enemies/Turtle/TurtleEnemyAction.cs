using UnityEngine;

public class TurtleEnemyAction : MonoBehaviour // <--- Now it can be attached!
{
    // Drag your "TurtleData" asset here in the Inspector
    public StaticDataEnemies enemyData;

    void Update()
    {
        // Example: Access data through the variable
        // float currentSpeed = enemyData.moveSpeed; 
    }

    public void MoveToPlayer() { }
    public void AttackPlayer() { }
    public void OnDestroy() { }
}