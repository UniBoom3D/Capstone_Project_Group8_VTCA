using System;

public class EXPCharacterManager
{
    public int CurrentExp { get; private set; }
    public int MaxExp { get; private set; }

    public EXPCharacterManager()
    {
        CurrentExp = 0;
        MaxExp = 100; // Mức EXP ban đầu để lên level 2
    }

    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        CurrentExp += amount;

        // Nếu EXP vượt quá max, tăng level và tính lại max EXP
        while (CurrentExp >= MaxExp)
        {
            CurrentExp -= MaxExp;
            IncreaseLevel();
        }
    }

    private void IncreaseLevel()
    {
        // Cập nhật lại max EXP theo level
        int level = GetLevel();
        MaxExp = CalculateMaxExp(level + 1);
    }

    public int GetLevel()
    {       
        int level = (CurrentExp / MaxExp) + 1;
        return level > 20 ? 20 : level; // Giới hạn tối đa là level 20
    }

    public int CalculateMaxExp(int level)
    {
        // Công thức tính max EXP cho mỗi level (có thể thay đổi tùy thuộc vào công thức bạn muốn)
        return 100 + (level - 1) * 10;
    }
}
