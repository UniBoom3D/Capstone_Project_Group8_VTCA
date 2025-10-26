using System;

[System.Serializable]
public class StaticPlayerData
{
    // Hệ thống tạo tự động khi tạo tài khoản
    public string _playerID;
    public string _username;
    public string _password;

    // Dữ liệu nhân vật
    public string _characterName;  
    public string _characterID;   

    public int _level = 1;
}

