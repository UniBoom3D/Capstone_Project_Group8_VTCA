using UnityEngine;

public abstract class StaticDataCharacter : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string className;
    public string description;
    public Sprite icon;

    [Header("Chỉ số khởi đầu (Level 1)")]
    public int _baseHealth;
    public int _baseStamina;
    public int _baseAttack;
    public int _baseMagic;
    public int _baseArmor;
    public int _baseMagicResist;

    /// <summary>
    /// Mỗi lớp con phải tự định nghĩa công thức tăng chỉ số theo cấp.
    /// </summary>
    public abstract BasicStats GetStatsAtLevel(int level);
}
