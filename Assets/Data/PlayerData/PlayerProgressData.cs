using UnityEngine;

[System.Serializable]
public class PlayerProgressData : StaticPlayerData
{
    [Header("Progress Data")]
    public int _currentLevel = 1;
    public int _currentExp = 0;
    public int _maxExp = 100;

    public int _health;
    public int _stamina;
    public int _attack;
    public int _magic;
    public int _armor;
    public int _magicResist;

    public void SetCharacterSelection(string characterId, string characterName)
    {
        _characterID = characterId;
        this.characterName = characterName;
    }

}
