using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvatarData
{
    public string avatarID;
    public Sprite avatarSprite;
}

[CreateAssetMenu(fileName = "AvatarDatabase", menuName = "Game/Avatar Database")]
public class AvatarDataList : ScriptableObject
{
    public List<AvatarData> avatars;

    public Sprite GetAvatar(string id)
    {
        var avatar = avatars.Find(a => a.avatarID == id);

        if (avatar != null)
            return avatar.avatarSprite;

        Debug.LogWarning("Avatar ID not found: " + id);
        return null;
    }
}
