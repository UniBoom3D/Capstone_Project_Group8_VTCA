using UnityEngine;

public abstract class StaticDataEnemies : ScriptableObject
{
    [Header("Thông tin")]
    public string className;
    public string description;
    public Sprite icon;

    [Header("Thông số")]
    public int _baseHealth;
    public int _baseStamina;
    public int _baseAttack;
    public int _baseMagic;
    public int _baseArmor;
    public int _baseMagicResist;

   

}
