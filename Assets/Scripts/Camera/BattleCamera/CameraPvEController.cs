using UnityEngine;

public class CameraPvEController : MonoBehaviour
{
    public static CameraPvEController Instance { get; private set; }

    [SerializeField] private Camera mainCamera;

    [Header("Offsets")]
    [SerializeField] private Vector3 enemyOffset = new Vector3(0, 3, -5);
    [SerializeField] private Vector3 playerOffset = new Vector3(0, 4, -6);

    private void Awake()
    {
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void FocusEnemy(Transform target)
    {
        if (target == null) return;

        mainCamera.transform.position = target.position + enemyOffset;
        mainCamera.transform.LookAt(target);
    }

    public void FocusPlayer(Transform target)
    {
        if (target == null) return;

        mainCamera.transform.position = target.position + playerOffset;
        mainCamera.transform.LookAt(target);
    }

    public void LookAt(Transform target)
    {
        if (target == null) return;

        mainCamera.transform.LookAt(target);
    }
}