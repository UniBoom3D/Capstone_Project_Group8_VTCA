using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillUIController : MonoBehaviour
{
    public SkillManager skillManager;

    public Transform skillContainer;
    public GameObject skillIconPrefab;

    private List<GameObject> activeIcons = new List<GameObject>();

    private void OnEnable()
    {
        skillManager.OnSkillActivated += AddSkillIcon;
        skillManager.OnSkillsConsumed += ClearSkillIcons;
    }

    private void OnDisable()
    {
        skillManager.OnSkillActivated -= AddSkillIcon;
        skillManager.OnSkillsConsumed -= ClearSkillIcons;
    }

    private void AddSkillIcon(SkillData skill)
    {
        GameObject icon = Instantiate(skillIconPrefab, skillContainer);

        Text text = icon.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = skill.skillName;
        }

        activeIcons.Add(icon);
    }

    private void ClearSkillIcons()
    {
        foreach (var icon in activeIcons)
        {
            Destroy(icon);
        }

        activeIcons.Clear();
    }
}