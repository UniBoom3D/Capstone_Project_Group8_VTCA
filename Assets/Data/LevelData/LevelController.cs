using UnityEngine;

public class LevelController
{
    private PlayerProgressData _playerData;
    private System.Random _random;

    public LevelController(PlayerProgressData playerData)
    {
        _playerData = playerData;
        _random = new System.Random();
    }

    // Add EXP
    public void AddExp(int amount)
    {
        _playerData._currentExp += amount;

        while (_playerData._currentExp >= _playerData._maxExp)
        {
            _playerData._currentExp -= _playerData._maxExp;
            LevelUp();
        }
    }

    // Handle level up
    private void LevelUp()
    {
        _playerData._currentLevel++;

        // Increase each stat randomly from 1-3
        int healthIncrease = _random.Next(1, 4);      // 1-3
        int staminaIncrease = _random.Next(1, 4);
        int attackIncrease = _random.Next(1, 4);
        int magicIncrease = _random.Next(1, 4);
        int armorIncrease = _random.Next(1, 4);
        int magicResistIncrease = _random.Next(1, 4);

        _playerData._health += healthIncrease;
        _playerData._stamina += staminaIncrease;
        _playerData._attack += attackIncrease;
        _playerData._magic += magicIncrease;
        _playerData._armor += armorIncrease;
        _playerData._magicResist += magicResistIncrease;

        Debug.Log($"LEVEL UP! New Level: {_playerData._currentLevel}");
        Debug.Log($"Stats Increased: Health+{healthIncrease}, Stamina+{staminaIncrease}, Attack+{attackIncrease}, Magic+{magicIncrease}, Armor+{armorIncrease}, MagicResist+{magicResistIncrease}");
    }
}
