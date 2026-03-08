using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleTeamData
{
    public string TeamName { get; private set; }

    private List<ITurnParticipant> members = new List<ITurnParticipant>();

    public IReadOnlyList<ITurnParticipant> Members => members;

    public BattleTeamData(string name)
    {
        TeamName = name;
    }

    // =========================
    // ADD / REMOVE
    // =========================

    public void AddMember(ITurnParticipant participant)
    {
        if (participant == null) return;

        if (!members.Contains(participant))
        {
            members.Add(participant);
        }
    }

    public void RemoveMember(ITurnParticipant participant)
    {
        if (participant == null) return;

        members.Remove(participant);
    }

    // =========================
    // QUERIES
    // =========================

    public List<ITurnParticipant> GetAliveMembers()
    {
        return members
            .Where(m => m != null && m.IsAlive)
            .ToList();
    }

    public ITurnParticipant GetRandomAliveMember()
    {
        var alive = GetAliveMembers();

        if (alive.Count == 0)
            return null;

        return alive[Random.Range(0, alive.Count)];
    }

    public bool IsDefeated
    {
        get
        {
            return members.All(m => m == null || !m.IsAlive);
        }
    }

    public int AliveCount
    {
        get
        {
            return members.Count(m => m != null && m.IsAlive);
        }
    }
}