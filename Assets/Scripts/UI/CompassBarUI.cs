using UnityEngine;
using UnityEngine.UI;

public class CompassBarUI : MonoBehaviour
{
    public RawImage compassImage;   
    public Transform player;        
    public float compassUnit = 0.0025f; 

    void Update()
    {
        // Lấy hướng quay Y của player
        float playerRot = player.eulerAngles.y;

        // Offset texture theo góc xoay
        compassImage.uvRect = new Rect(playerRot * compassUnit, 0, 1, 1);
    }
}
