using System;

[Serializable]
public class CharacterProgressData
{
    // ===========================
    // 🆔 IDENTIFY CHARACTER
    // ===========================
    public string characterID;        // GUID duy nhất
    public string characterName;      // Tên người chơi đặt
    public string characterClass;     // Archer / Mage / Gunner

    // ===========================
    // 📈 PROGRESSION
    // ===========================
    public int level = 1;
    public int exp = 0;
    public int maxExp = 100;

    // ===========================
    // 💪 RUNTIME STATS
    // ===========================
    public int health;
    public int stamina;
    public int attack;
    public int magic;
    public int armor;
    public int magicResist;

    // ===========================
    // 🧩 ÁP DỤNG STAT BAN ĐẦU
    // ===========================
    public void ApplyBaseStats(BasicStats stats)
    {
        health = stats._health;
        stamina = stats._stamina;
        attack = stats._attack;
        magic = stats._magic;
        armor = stats._armor;
        magicResist = stats._magicResist;
    }

    // ===========================
    // 🔄 RESET STAT (RECALCULATE)
    // ===========================
    public void ResetStats(BasicStats stats)
    {
        ApplyBaseStats(stats);
    }

    // ===========================
    // 📈 EXP & LEVEL SYSTEM
    // ===========================
    public void AddExp(int amount)
    {
        if (amount <= 0) return; // tránh exp âm

        exp += amount;

        while (exp >= maxExp)
        {
            exp -= maxExp;
            level++;
            maxExp = CalculateMaxExp(level);
        }
    }

    public int CalculateMaxExp(int level)
    {
        if (level < 1) level = 1;
        return 100 + (level - 1) * 25;
    }
}
