using UnityEngine;

[CreateAssetMenu(fileName = "New Character Static Data", menuName = "Character/Static Data")]
public class StaticDataCharacter : ScriptableObject
{
    [Header("Tên Nhân Vật")]
    public string characterName;

    [Header("Chỉ số khởi đầu")]
    public int _baseHealth;
    public int _baseAttack;
    public int _baseMagic;
    public int _baseArmor;
    public int _baseMagicResist;
    public int _baseStamina;
}
