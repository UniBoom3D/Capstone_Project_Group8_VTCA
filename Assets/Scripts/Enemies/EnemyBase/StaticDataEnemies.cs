using UnityEngine;

public abstract class StaticDataEnemies : ScriptableObject
{
    [Header("Info")]
    public string className;
    public string description;
    public Sprite icon;

    [Header("Base Stats")]
    public int baseHealth;
    public int baseStamina;
    public int baseAttack;
    public int baseMagic;
    public int baseArmor;
    public int baseMagicResist;

    [Header("Combat")]
    public float moveSpeed;
    public float attackRange;
}