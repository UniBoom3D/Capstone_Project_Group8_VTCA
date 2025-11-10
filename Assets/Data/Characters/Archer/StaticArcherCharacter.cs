using UnityEngine;

[CreateAssetMenu(fileName = "ArcherCharacterData", menuName = "Character/Static/Archer")]
public class StaticArcherCharacter : StaticDataCharacter
{
    private void OnEnable()
    {
        characterName = "Archer";
        _baseHealth = 80;
        _baseStamina = 100;
        _baseAttack = 15;
        _baseMagic = 5;
        _baseArmor = 8;
        _baseMagicResist = 6;
    }

    public override PlayerProgressData.BasicStats GetStatsAtLevel(int level)
    {
        level = Mathf.Max(1, level);
        int lv = level - 1;

        // Archer: tăng nhanh công, máu và thể lực trung bình
        return new PlayerProgressData.BasicStats
        {
            health = _baseHealth + 8 * lv,
            stamina = _baseStamina + 5 * lv,
            attack = _baseAttack + 4 * lv,
            magic = _baseMagic + 1 * lv,
            armor = _baseArmor + 2 * lv,
            magicResist = _baseMagicResist + 1 * lv
        };
    }
}
