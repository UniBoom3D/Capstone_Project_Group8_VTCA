using System;

[System.Serializable]
public class StaticPlayerData
{
    // Hệ thống tạo tự động khi tạo tài khoản
    public string _playerID;
    public string username;
    public string password;

    // Dữ liệu nhân vật
    public string characterName;  
    public string _characterID;   

    public int level = 1;
}
