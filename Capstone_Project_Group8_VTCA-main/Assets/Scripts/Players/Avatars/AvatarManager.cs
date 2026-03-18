using UnityEngine;
using UnityEngine.UI;

public class AvatarManager : MonoBehaviour
{
    [Header("Main UI")]
    public GameObject setupAvatarPanel; // Panel chứa danh sách avatar
    public Button confirmButton;       // Nút xác nhận
    public Transform container;

    [Header("References")]
    public AvatarDataList avatarDatabase;
    public Image playerAvatarImage;    // Ảnh đại diện đang hiển thị ngoài màn hình chính

    [Header("Selection Effect")]
    public Image selectionCircle;      // GameObject vòng tròn xanh lá (Image)

    private string currentSelectedID;  // ID đang chọn (nhưng chưa bấm Confirm)
    private const string AVATAR_KEY = "AvatarID";
    private const string DEFAULT_AVATAR_ID = "avatar_lion";

    private void Start()
    {
        LoadAvatar();

        // Ẩn panel và vòng tròn lúc đầu
        if (setupAvatarPanel != null) setupAvatarPanel.SetActive(false);
        if (selectionCircle != null) selectionCircle.gameObject.SetActive(false);

        // Gán sự kiện cho nút Confirm
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmSelection);
    }

    // 1. Hàm mở Panel (Gán vào Event Trigger của Player Avatar Image)
    public void OpenSetupPanel()
    {
        setupAvatarPanel.SetActive(true);

        // Lấy ID hiện tại đang dùng từ PlayerPrefs
        string savedID = PlayerPrefs.GetString(AVATAR_KEY, DEFAULT_AVATAR_ID);
        currentSelectedID = savedID;

        // Tự động tìm GameObject có tên trùng với savedID trong danh sách con
        // Chú ý: Container ở đây là Viewport (hoặc Content) của bạn
        Transform target = container.Find(savedID);

        if (target != null && selectionCircle != null)
        {
            selectionCircle.gameObject.SetActive(true);
            selectionCircle.transform.SetParent(target);
            selectionCircle.rectTransform.anchoredPosition = Vector2.zero;
            selectionCircle.transform.SetAsLastSibling();
        }
    }

    // 2. Hàm khi Click vào từng Image trong danh sách
    // Tham số 'targetTransform' dùng để lấy vị trí vẽ vòng tròn
    public void SelectAvatar(string id)
    {
        currentSelectedID = id;

        // Tìm xem Image nào trong danh sách đang được click thông qua EventSystem
        GameObject clickedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (clickedObject != null && selectionCircle != null)
        {
            selectionCircle.gameObject.SetActive(true);

            // Di chuyển vòng tròn đến đối tượng vừa click
            selectionCircle.transform.SetParent(clickedObject.transform);
            selectionCircle.rectTransform.anchoredPosition = Vector2.zero;

            // Đảm bảo vòng tròn nằm trên cùng trong phân cấp của ảnh đó
            selectionCircle.transform.SetAsLastSibling();
        }
    }

    // 3. Hàm khi bấm nút Confirm
    public void ConfirmSelection()
    {
        SetAvatar(currentSelectedID);
        setupAvatarPanel.SetActive(false);
    }

    public void SetAvatar(string id)
    {
        if (avatarDatabase == null) return;

        Sprite avatar = avatarDatabase.GetAvatar(id);
        if (avatar != null)
        {
            playerAvatarImage.sprite = avatar;
            PlayerPrefs.SetString(AVATAR_KEY, id);
            PlayerPrefs.Save();
        }
    }

    private void LoadAvatar()
    {
        string savedID = PlayerPrefs.GetString(AVATAR_KEY, DEFAULT_AVATAR_ID);
        SetAvatar(savedID);
    }
}