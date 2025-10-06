using UnityEngine;

public class PlayerInBattle : ITurnParticipant
{
    public string Name { get; private set; }
    public int HP { get; private set; }
    public int AttackPower { get; private set; }
    public int Magic { get; private set; }
    public int Armor { get; private set; }
    public int MagicResist { get; private set; }

    public bool IsAlive => HP > 0;

    public PlayerInBattle(PlayerProgressData data)
    {
        Name = data.characterName;
        HP = data._health;
        AttackPower = data._attack;
        Magic = data._magic;
        Armor = data._armor;
        MagicResist = data._magicResist;
    }

    public void TakeTurn()
    {
        UnityEngine.Debug.Log($"{Name} is taking a turn!");
        // TODO: Chọn hành động (Attack, Skill, Item…)
    }

    public void TakeDamage(int dmg)
    {
        int actualDamage = Mathf.Max(dmg - Armor, 1); // có giáp giảm sát thương
        HP -= actualDamage;
        if (HP < 0) HP = 0;

        UnityEngine.Debug.Log($"{Name} took {actualDamage} damage. HP left: {HP}");
    }
}
