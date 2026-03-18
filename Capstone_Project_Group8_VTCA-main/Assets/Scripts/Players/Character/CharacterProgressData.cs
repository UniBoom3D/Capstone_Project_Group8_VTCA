using System;

[Serializable]
public class CharacterProgressData
{
    public string characterId;
    public string characterName;
    public string characterClass;
    public int level = 1;
    public int exp = 0;
    public int maxExp = 100;

    // Tham chiếu đến các script quản lý level và EXP
    private LevelCharacterManager levelCharacterManager;

    // Tham chiếu đến StaticDataCharacter
    private StaticDataCharacter characterStaticData;

    public BasicStats characterStats;

    // Constructor nhận StaticDataCharacter (base stats của nhân vật)
    public CharacterProgressData(StaticDataCharacter staticData)
    {
        characterStaticData = staticData;
        levelCharacterManager = new LevelCharacterManager();  
        characterStats = characterStaticData.GetStatsAtLevel(level);
    }

    public void AddExp(int amount)
    {
        levelCharacterManager.AddExp(amount);

        // Cập nhật lại thông tin stats khi level tăng
        level = levelCharacterManager.GetCurrentLevel();
        characterStats = characterStaticData.GetStatsAtLevel(level);
    }

    public int GetCurrentLevel()
    {
        return levelCharacterManager.GetCurrentLevel();
    }

    public int GetCurrentExp()
    {
        return levelCharacterManager.GetCurrentExp();
    }

    public int GetMaxExp()
    {
        return levelCharacterManager.GetMaxExp();
    }
}
