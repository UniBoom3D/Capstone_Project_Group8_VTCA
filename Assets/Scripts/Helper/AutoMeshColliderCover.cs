using UnityEngine;

public class AutoMeshColliderCover : MonoBehaviour
{
    [ContextMenu("List Mesh Renderer Children")]
    public void ListChildren()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);

        Debug.Log("===== LIST MESH RENDERER CHILDREN =====");

        foreach (MeshRenderer r in renderers)
        {
            Debug.Log(GetFullPath(r.transform));
        }

        Debug.Log("===== TOTAL: " + renderers.Length + " =====");
    }

    string GetFullPath(Transform t)
    {
        string path = t.name;

        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }
}