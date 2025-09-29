using System.Collections.Generic;
using UnityEngine;

public enum State
{
    None_State = 0,
    Blue_Team_State = 1,
    Red_Team_State = 2
}


public class StaticBattleData
{
    public StaticBattleData Instance;

    public string BlueTeam;

    public string RedTeam;

    private void Setup()
    {
        Instance = this;
        BlueTeam = "Blue Team";
        RedTeam = "Red Team";
    }

}
