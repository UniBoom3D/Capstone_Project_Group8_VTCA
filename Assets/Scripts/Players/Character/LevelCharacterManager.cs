using System;

public class LevelCharacterManager
{
    private EXPCharacterManager expCharacterManager;

    public LevelCharacterManager()
    {
        expCharacterManager = new EXPCharacterManager();
    }

    public int GetCurrentLevel()
    {
        return expCharacterManager.GetLevel();
    }

    public void AddExp(int amount)
    {
        expCharacterManager.AddExp(amount);
    }

    public int GetCurrentExp()
    {
        return expCharacterManager.CurrentExp;
    }

    public int GetMaxExp()
    {
        return expCharacterManager.MaxExp;
    }
}
