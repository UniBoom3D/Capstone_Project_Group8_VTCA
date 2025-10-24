using UnityEngine;

[System.Serializable]
public class PlayerProgressData : StaticPlayerData
{
    [Header("Progress Data")]
    public int _currentLevel = 1;
    public int _currentExp = 0;
    public int _maxExp = 100;

    [Header("Base Stats")]
    public int _health;
    public int _stamina;
    public int _attack;
    public int _magic;
    public int _armor;
    public int _magicResist;

    public void SetCharacterSelection(string characterId, string characterName)
    {
        _characterID = characterId;
        this._characterName = characterName;
    }

    /// <summary>
    /// Trả về struct chứa chỉ số cơ bản để truyền vào Battle
    /// </summary>
    public BasicStats GetBaseStats()
    {
        return new BasicStats
        {
            health = _health,
            stamina = _stamina,
            attack = _attack,
            magic = _magic,
            armor = _armor,
            magicResist = _magicResist
        };
    }
    public struct BasicStats
    {
        public int health;
        public int stamina;
        public int attack;
        public int magic;
        public int armor;
        public int magicResist;
    }
}
