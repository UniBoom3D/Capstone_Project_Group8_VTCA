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
}
