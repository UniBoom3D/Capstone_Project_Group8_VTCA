using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    void Awake()
    {
        Instance = this;
    }

    // =========================
    // GENERIC SPAWN
    // =========================

    public GameObject SpawnUnit(
        GameObject prefab,
        Transform spawnPoint)
    {
        GameObject unit = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        return unit;
    }

    // =========================
    // SPAWN TURTLE FROM DATA
    // =========================

    public TurtleEnemyAction SpawnTurtle(
        TurtleData data,
        Transform spawnPoint)
    {
        GameObject unit = Instantiate(
            data.turtlePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        TurtleEnemyAction turtle =
            unit.GetComponent<TurtleEnemyAction>();

        if (turtle != null)
        {
            turtle.enemyData = data;

            TurtlePvEManager.Instance.RegisterTurtle(turtle);
        }

        return turtle;
    }
}