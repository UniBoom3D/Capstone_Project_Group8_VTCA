using System;

[Serializable]
public struct BasicStats
{
    public int _health;
    public int _stamina;
    public int _attack;
    public int _magic;
    public int _armor;
    public int _magicResist;

    public BasicStats(int health, int stamina, int attack, int magic, int armor, int magicResist)
    {
        _health = health;
        _stamina = stamina;
        _attack = attack;
        _magic = magic;
        _armor = armor;
        _magicResist = magicResist;
    }
}