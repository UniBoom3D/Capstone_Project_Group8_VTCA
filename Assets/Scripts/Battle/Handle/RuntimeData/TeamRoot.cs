using System.Collections.Generic;
using UnityEngine;

public class TeamRoot : MonoBehaviour
{
    public List<ITurnParticipant> members = new();

    public void Register(ITurnParticipant unit)
    {
        if (!members.Contains(unit))
            members.Add(unit);
    }

    public void Unregister(ITurnParticipant unit)
    {
        members.Remove(unit);
    }
}