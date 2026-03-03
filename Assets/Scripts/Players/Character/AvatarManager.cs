using UnityEngine;
using UnityEngine.UI;

public class AvatarManager : MonoBehaviour
{
    [Header("References")]
    public AvatarDataList avatarDatabase;
    public Image playerAvatarImage;

    private const string AVATAR_KEY = "AvatarID";
    private const string DEFAULT_AVATAR_ID = "avatar_lion";

    private void Start()
    {
        LoadAvatar();
    }

    public void SetAvatar(string id)
    {
        if (avatarDatabase == null)
        {
            Debug.LogError("Avatar Database not assigned!");
            return;
        }

        Sprite avatar = avatarDatabase.GetAvatar(id);

        if (avatar != null)
        {
            playerAvatarImage.sprite = avatar;

            // Save local
            PlayerPrefs.SetString(AVATAR_KEY, id);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("Avatar not found, loading default.");
            SetDefaultAvatar();
        }
    }

    private void LoadAvatar()
    {
        string savedID = PlayerPrefs.GetString(AVATAR_KEY, DEFAULT_AVATAR_ID);
        SetAvatar(savedID);
    }

    private void SetDefaultAvatar()
    {
        Sprite defaultAvatar = avatarDatabase.GetAvatar(DEFAULT_AVATAR_ID);

        if (defaultAvatar != null)
            playerAvatarImage.sprite = defaultAvatar;
    }
}