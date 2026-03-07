using UnityEngine;

[CreateAssetMenu(fileName = "TurtleData", menuName = "Game/Enemies/Turtle Data")]
public class TurtleData : EnemyBaseData
{
    [Header("Turtle")]
    public GameObject turtlePrefab;
}