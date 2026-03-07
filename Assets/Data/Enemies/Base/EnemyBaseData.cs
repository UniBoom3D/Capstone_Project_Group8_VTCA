using UnityEngine;

public abstract class EnemyBaseData : ScriptableObject
{
    [Header("Info")]
    public string className;
    public string description;
    public Sprite icon;

    [Header("Base Stats")]
    public float baseHealth;
    public float baseStamina;
    public float baseAttack;
    public float baseMagic;
    public float baseArmor;
    public float baseMagicResist;

    [Header("Combat")]
    public float moveSpeed;
    public float attackRange;

    [Header("Projectile")]
    public float projectilePower;
    public float projectileArcHeight;

    [Header("AI")]
    public float aimTime;
    public float aimAccuracy;
}