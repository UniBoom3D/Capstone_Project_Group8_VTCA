using UnityEngine;

public class SpawnMap : MonoBehaviour
{
    [SerializeField] private GameObject mapPrefab;

    public Transform CurrentMap { get; private set; }

    public void Spawn()
    {
        if (mapPrefab == null)
        {
            Debug.LogError("Map prefab missing");
            return;
        }

        GameObject map = Instantiate(mapPrefab);

        CurrentMap = map.transform;

        Debug.Log("🌍 Map Spawned");
    }
}