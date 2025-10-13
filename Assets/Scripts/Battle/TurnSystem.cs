using System.Collections.Generic;
using UnityEngine;

public class TurnSystem
{
    public ITurnParticipant CurrentParticipant { get; private set; }

    private Queue<ITurnParticipant> turnQueue = new Queue<ITurnParticipant>();

    public void Initialize(List<ITurnParticipant> participants)
    {
        turnQueue = new Queue<ITurnParticipant>(participants);
    }

    public void NextTurn()
    {
        if (turnQueue.Count == 0) return;

        CurrentParticipant = turnQueue.Dequeue();
        if (!CurrentParticipant.IsAlive)
        {
            return; // bỏ qua nếu đã chết
        }

        Debug.Log($"--- {CurrentParticipant.Name}'s Turn ---");
        ExecuteTurn(CurrentParticipant);
        turnQueue.Enqueue(CurrentParticipant);
    }

    private void ExecuteTurn(ITurnParticipant participant)
    {
        participant.TakeTurn();
    }

}
