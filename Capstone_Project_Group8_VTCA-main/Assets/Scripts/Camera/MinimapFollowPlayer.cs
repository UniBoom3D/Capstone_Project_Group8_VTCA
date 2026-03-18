using UnityEngine;

public class MinimapFollowPlayer : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 newPos = target.position;
        newPos.y = transform.position.y; // giữ nguyên chiều cao
        transform.position = newPos;
    }
}
