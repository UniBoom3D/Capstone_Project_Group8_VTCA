using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtlePvEManager : MonoBehaviour
{
    public static TurtlePvEManager Instance;

    private List<TurtleEnemyAction> turtleUnits =
        new List<TurtleEnemyAction>();

    private bool turnFinished;

    void Awake()
    {
        Instance = this;
    }

    // =========================
    // INIT BATTLE
    // =========================

    public void InitializeTurtleBattle()
    {
        turtleUnits.Clear();

        TurtleEnemyAction[] turtles =
            FindObjectsOfType<TurtleEnemyAction>();

        foreach (var turtle in turtles)
        {
            RegisterTurtle(turtle);
        }

        Debug.Log($"🐢 Registered turtles: {turtleUnits.Count}");
    }

    // =========================
    // REGISTER
    // =========================

    public void RegisterTurtle(TurtleEnemyAction turtle)
    {
        if (!turtleUnits.Contains(turtle))
            turtleUnits.Add(turtle);
    }

    // =========================
    // EXECUTE ENEMY TURN
    // =========================

    public IEnumerator ExecuteTurtleEnemyTurn()
    {
        foreach (var turtle in turtleUnits)
        {
            if (!turtle.IsAlive)
                continue;

            turnFinished = false;

            turtle.TakeTurn();

            yield return new WaitUntil(() => turnFinished);
        }
    }

    // =========================
    // CALLBACK
    // =========================

    public void NotifyTurnFinished()
    {
        turnFinished = true;
    }
}