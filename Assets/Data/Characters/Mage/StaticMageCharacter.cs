using UnityEngine;

[CreateAssetMenu(fileName = "MageCharacterData", menuName = "Character/Static/Mage")]
public class StaticMageCharacter : StaticDataCharacter
{
    private void OnEnable()
    {
        className = "Mage";
        _baseHealth = 70;
        _baseStamina = 100;
        _baseAttack = 5;
        _baseMagic = 20;
        _baseArmor = 5;
        _baseMagicResist = 12;
    }

    public override BasicStats GetStatsAtLevel(int level)
    {
        level = Mathf.Max(1, level);
        int lv = level - 1;

        return new BasicStats
        {
            _health = _baseHealth + 5 * lv,
            _stamina = _baseStamina + 4 * lv,
            _attack = _baseAttack + 1 * lv,
            _magic = _baseMagic + 6 * lv,
            _armor = _baseArmor + 1 * lv,
            _magicResist = _baseMagicResist + 3 * lv
        };
    }
}
