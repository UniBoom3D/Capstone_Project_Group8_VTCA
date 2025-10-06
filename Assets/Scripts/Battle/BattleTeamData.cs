using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public class BattleTeamData
{
    public string TeamName;
    public List<ITurnParticipant> Members = new List<ITurnParticipant>();

    public bool IsDefeated => Members.All(m => !m.IsAlive);

    public BattleTeamData(string name)
    {
        TeamName = name;
    }

    public void AddMember(ITurnParticipant participant)
    {
        Members.Add(participant);
    }

    public ITurnParticipant GetRandomAliveMember()
    {
        var alive = Members.Where(m => m.IsAlive).ToList();
        return alive.Count > 0 ? alive[Random.Range(0, alive.Count)] : null;
    }
}
