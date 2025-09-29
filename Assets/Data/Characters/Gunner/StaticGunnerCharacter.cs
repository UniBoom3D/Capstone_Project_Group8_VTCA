using UnityEngine;

[CreateAssetMenu(fileName = "GunnerCharacterData", menuName = "Character/Static/Gunner")]
public class StaticGunnerCharacter : StaticDataCharacter
{
    private void OnEnable()
    {
        characterName = "Gunner";

        _baseHealth = 90;
        _baseAttack = 18;
        _baseMagic = 0;
        _baseArmor = 10;
        _baseMagicResist = 4;
        _baseStamina = 110;
    }
}
