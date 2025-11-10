using UnityEngine;

[CreateAssetMenu(fileName = "MageCharacterData", menuName = "Character/Static/Mage")]
public class StaticMageCharacter : StaticDataCharacter
{
    private void OnEnable()
    {
        characterName = "Mage";
        _baseHealth = 70;
        _baseStamina = 100;
        _baseAttack = 5;
        _baseMagic = 20;
        _baseArmor = 5;
        _baseMagicResist = 12;
    }

    public override PlayerProgressData.BasicStats GetStatsAtLevel(int level)
    {
        level = Mathf.Max(1, level);
        int lv = level - 1;

        // Mage: tăng nhanh magic và magicResist, máu yếu
        return new PlayerProgressData.BasicStats
        {
            health = _baseHealth + 5 * lv,
            stamina = _baseStamina + 4 * lv,
            attack = _baseAttack + 1 * lv,
            magic = _baseMagic + 6 * lv,
            armor = _baseArmor + 1 * lv,
            magicResist = _baseMagicResist + 3 * lv
        };
    }
}
