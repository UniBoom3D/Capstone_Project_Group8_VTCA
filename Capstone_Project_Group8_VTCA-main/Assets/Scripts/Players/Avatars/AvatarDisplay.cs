using UnityEngine;
using UnityEngine.UI;

public class AvatarDisplay : MonoBehaviour
{
    [Header("References")]
    public AvatarDataList avatarDatabase; // Kéo ScriptableObject Database vào đây
    public Image displayImage;           // Image sẽ hiển thị Avatar

    private const string AVATAR_KEY = "AvatarID";
    private const string DEFAULT_AVATAR_ID = "avatar_lion";

    private void Start()
    {
        RefreshDisplay();
    }

    // Hàm này dùng để cập nhật hình ảnh dựa trên ID đã lưu trong PlayerPrefs
    public void RefreshDisplay()
    {
        if (avatarDatabase == null || displayImage == null)
        {
            Debug.LogWarning("AvatarDisplay: Thiếu references!");
            return;
        }

        // 1. Đọc ID từ PlayerPrefs
        string savedID = PlayerPrefs.GetString(AVATAR_KEY, DEFAULT_AVATAR_ID);

        // 2. Lấy Sprite tương ứng từ Database
        Sprite avatarSprite = avatarDatabase.GetAvatar(savedID);

        // 3. Hiển thị lên UI
        if (avatarSprite != null)
        {
            displayImage.sprite = avatarSprite;
        }
        else
        {
            Debug.LogError($"AvatarDisplay: Không tìm thấy Sprite cho ID {savedID}");
        }
    }
}