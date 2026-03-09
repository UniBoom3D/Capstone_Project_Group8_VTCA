using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurtleLoadingManager : MonoBehaviour
{
    [Header("Scenes")]
    public string mapScene = "TurtleMap";
    public string battleScene = "TurtleBattleScene";

    void Start()
    {
        StartCoroutine(LoadRoutine());
    }

    IEnumerator LoadRoutine()
    {
        Debug.Log("Loading Start");

        // 1️⃣ Read session data
        var session = BattleTurtleRoom.Instance.GetSession();

        if (session == null)
        {
            Debug.LogError("SessionData missing");
            yield break;
        }

        // 2️⃣ Load Map
        AsyncOperation mapLoad =
            SceneManager.LoadSceneAsync(mapScene, LoadSceneMode.Additive);

        while (!mapLoad.isDone)
            yield return null;

        // 3️⃣ Load Battle Scene
        AsyncOperation battleLoad =
            SceneManager.LoadSceneAsync(battleScene, LoadSceneMode.Additive);

        while (!battleLoad.isDone)
            yield return null;

        Debug.Log("Battle Scene Ready");

        // set active scene
        Scene battle = SceneManager.GetSceneByName(battleScene);
        SceneManager.SetActiveScene(battle);

        // 4️⃣ unload loading scene
        SceneManager.UnloadSceneAsync("TurtleLoadingScene");
    }
}