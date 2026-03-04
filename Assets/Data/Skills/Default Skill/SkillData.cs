using UnityEngine;

public enum SkillType
{
    MultiShot,     // bắn nhiều viên
    SquareShot,    // 9 viên hình vuông
    DamageBuff     // buff damage
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillID;
    public string skillName;
    public SkillType skillType;

    [Header("Combat Settings")]
    public int projectileCount = 1;
    [Range(0f, 2f)]
    public float damageMultiplier = 1f;

    [Header("Cooldown & Duration")]
    public float cooldown = 3f;
    public float duration = 0f; // chỉ dùng cho buff
}