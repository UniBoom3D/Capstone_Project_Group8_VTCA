using UnityEngine;

[CreateAssetMenu(fileName = "GunnerCharacterData", menuName = "Character/Static/Gunner")]
public class StaticGunnerCharacter : StaticDataCharacter
{
    private void OnEnable()
    {
        characterName = "Gunner";
        _baseHealth = 90;
        _baseStamina = 100;
        _baseAttack = 18;
        _baseMagic = 0;
        _baseArmor = 10;
        _baseMagicResist = 4;
    }

    public override PlayerProgressData.BasicStats GetStatsAtLevel(int level)
    {
        level = Mathf.Max(1, level);
        int lv = level - 1;

        // Gunner: tank hơn, công ổn định, tăng armor mạnh
        return new PlayerProgressData.BasicStats
        {
            health = _baseHealth + 10 * lv,
            stamina = _baseStamina + 4 * lv,
            attack = _baseAttack + 3 * lv,
            magic = _baseMagic,
            armor = _baseArmor + 3 * lv,
            magicResist = _baseMagicResist + 1 * lv
        };
    }
}
