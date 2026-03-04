using UnityEngine;

public class ShotModifier
{
    public int projectileCount = 1;
    public float damageMultiplier = 1f;

    public void Combine(SkillData skill)
    {
        projectileCount *= skill.projectileCount;
        damageMultiplier *= skill.damageMultiplier;
    }
}
