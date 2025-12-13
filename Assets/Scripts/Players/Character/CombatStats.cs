using System;


[Serializable]
public struct CombatStats
{
    public int health;
    public int stamina;
    public int attack;
    public int magic;
    public int armor;
    public int magicResist;

    // Constructor để khởi tạo CombatStats từ CharacterProgressData
    public CombatStats(int health, int stamina, int attack, int magic, int armor, int magicResist)
    {
        this.health = health;
        this.stamina = stamina;
        this.attack = attack;
        this.magic = magic;
        this.armor = armor;
        this.magicResist = magicResist;
    }
}
