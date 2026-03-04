using System.Collections.Generic;
using UnityEngine;
using System;

public class SkillManager : MonoBehaviour
{
    public List<SkillData> defaultSkills;

    private List<SkillData> pendingSkills = new List<SkillData>();

    public event Action<SkillData> OnSkillActivated;
    public event Action OnSkillsConsumed;

    public void ActivateSkill(int index)
    {
        if (index < 0 || index >= defaultSkills.Count) return;

        SkillData skill = defaultSkills[index];

        pendingSkills.Add(skill);

        Debug.Log($"🧠 Skill {skill.skillName} activated (pending)");
        Debug.Log("💰 Saitama cost consumed (log only)");

        OnSkillActivated?.Invoke(skill);
    }

    public ShotModifier ConsumePendingSkills()
    {
        ShotModifier modifier = new ShotModifier();

        foreach (var skill in pendingSkills)
        {
            modifier.Combine(skill);
        }

        pendingSkills.Clear();

        OnSkillsConsumed?.Invoke();

        return modifier;
    }
}