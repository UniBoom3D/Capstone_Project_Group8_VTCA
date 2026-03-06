using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;


    private void Awake()
    {
        Instance = this;
    }

    public void InitializeBattle()
    {
        Debug.Log("⚙ Spawn System Start");

        

        Debug.Log("⚙ Spawn System Finished");
    }
}