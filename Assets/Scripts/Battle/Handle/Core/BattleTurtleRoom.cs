using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTurtleRoom : MonoBehaviour
{
    public static BattleTurtleRoom Instance;

    [Header("Scene")]
    public string loadingSceneName = "TurtleLoadingScene";

    [Header("Runtime Battle Data")]
    public BattleSessionPlayerData sessionData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public BattleSessionPlayerData GetSession()
    {
        return sessionData;
    }

    // =========================
    // START BATTLE
    // =========================

    public void StartBattle()
    {
        Debug.Log("Start Turtle Battle");

        sessionData = new BattleSessionPlayerData
        {
            playerId = "Player_01",
            playerLevel = 1,
            playerClass = "Warrior"
        };

        SceneManager.LoadScene(loadingSceneName);
    }

    // =========================
    // END BATTLE
    // =========================

    public void EndBattle()
    {
        sessionData = null;
    }

    // =========================
    // RETURN TO ROOM
    // =========================

    public void ReturnToRoom()
    {      
        EndBattle();

        Debug.Log("Return to TurtleRoomScene");
        SceneManager.LoadScene("TurtleRoomScene");
    }
}