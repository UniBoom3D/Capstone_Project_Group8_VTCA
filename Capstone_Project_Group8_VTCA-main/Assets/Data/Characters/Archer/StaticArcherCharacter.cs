using UnityEngine;

[CreateAssetMenu(fileName = "ArcherCharacterData", menuName = "Character/Static/Archer")]
public class StaticArcherCharacter : StaticDataCharacter
{
    private void OnEnable()
    {
        className = "Archer";
        _baseHealth = 80;
        _baseStamina = 100;
        _baseAttack = 15;
        _baseMagic = 5;
        _baseArmor = 8;
        _baseMagicResist = 6;
    }

    public override BasicStats GetStatsAtLevel(int level)
    {
        level = Mathf.Max(1, level);
        int lv = level - 1;

        return new BasicStats
        {
            _health = _baseHealth + 8 * lv,
            _stamina = _baseStamina + 5 * lv,
            _attack = _baseAttack + 4 * lv,
            _magic = _baseMagic + 1 * lv,
            _armor = _baseArmor + 2 * lv,
            _magicResist = _baseMagicResist + 1 * lv
        };
    }
}
