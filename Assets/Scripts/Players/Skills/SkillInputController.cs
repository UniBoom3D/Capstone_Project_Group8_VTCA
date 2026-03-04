using UnityEngine;

public class SkillInputController : MonoBehaviour
{
    public SkillManager skillManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            skillManager.ActivateSkill(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            skillManager.ActivateSkill(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            skillManager.ActivateSkill(2);
        }
    }
}