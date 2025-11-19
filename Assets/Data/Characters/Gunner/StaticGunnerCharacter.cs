using UnityEngine;

[CreateAssetMenu(fileName = "GunnerCharacterData", menuName = "Character/Static/Gunner")]
public class StaticGunnerCharacter : StaticDataCharacter
{
    private void OnEnable()
    {
        className = "Gunner";
        _baseHealth = 90;
        _baseStamina = 100;
        _baseAttack = 18;
        _baseMagic = 0;
        _baseArmor = 10;
        _baseMagicResist = 4;
    }

    public override BasicStats GetStatsAtLevel(int level)
    {
        level = Mathf.Max(1, level);
        int lv = level - 1;

        return new BasicStats
        {
            _health = _baseHealth + 10 * lv,
            _stamina = _baseStamina + 4 * lv,
            _attack = _baseAttack + 3 * lv,
            _magic = _baseMagic,
            _armor = _baseArmor + 3 * lv,
            _magicResist = _baseMagicResist + 1 * lv
        };
    }
}
