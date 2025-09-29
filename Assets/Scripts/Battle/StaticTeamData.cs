using System.Collections.Generic;
using UnityEngine;

public enum TeamID
{
    Blue_Team = 0,
    Red_Team = 1
}

public class StaticTeamData
{
    [SerializeField] private int _teamID;
    [SerializeField] private string _teamName;

    
}
