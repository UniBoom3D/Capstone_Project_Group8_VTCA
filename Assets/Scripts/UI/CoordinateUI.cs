using UnityEngine;
using UnityEngine.UI;

public class CoordinateUI : MonoBehaviour
{
    public Transform playerCamera;
    public Text coordText;     

    void Update()
    {
        float pitch = playerCamera.eulerAngles.x;

        if (pitch > 90 && pitch < 270)
            pitch = 90; // Giới hạn

        pitch = Mathf.Clamp(pitch, 0, 90);

        coordText.text = pitch.ToString("F0");
    }
}
