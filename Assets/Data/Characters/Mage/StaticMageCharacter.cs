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
}
