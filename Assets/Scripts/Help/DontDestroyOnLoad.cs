using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    private static GameObject _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this.gameObject;
            DontDestroyOnLoad(this);
            
        }
        else if (_instance != gameObject)
        {
            Debug.Log("Object bị hủy vì đã có instance tồn tại - " + gameObject.name);
            Destroy(gameObject);
        }
    }
}
