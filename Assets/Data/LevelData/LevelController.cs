using UnityEngine;
using System.Collections.Generic;

public static class LevelController
{
    // Định nghĩa max exp cho từng level
    private static Dictionary<int, int> levelExpTable = new Dictionary<int, int>()
    {
        { 1, 100 },
        { 2, 150 },
        { 3, 200 },
        { 4, 250 },
        { 5, 300 },
        // ... bạn có thể thêm tiếp nếu cần
    };

    // Hàm thêm exp cho player
    public static void AddExp(PlayerProgressData playerData, int amount)
    {
        playerData._currentExp += amount;

        int maxExp = GetMaxExpForLevel(playerData._currentLevel);

        if (playerData._currentExp >= maxExp)
        {
            LevelUp(playerData);
        }
    }

    // Lấy max exp cho level hiện tại
    private static int GetMaxExpForLevel(int level)
    {
        if (levelExpTable.ContainsKey(level))
            return levelExpTable[level];

        // Nếu level vượt bảng định nghĩa, mặc định tăng 50 mỗi cấp
        return 100 + (level - 1) * 50;
    }

    // Xử lý khi lên cấp
    private static void LevelUp(PlayerProgressData playerData)
    {
        playerData._currentLevel++;
        playerData._currentExp = 0; // reset exp sau khi lên cấp
        Debug.Log($"LEVEL UP! New Level: {playerData._currentLevel}");
    }
}
