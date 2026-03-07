using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [Header("Battle Handler")]
    public BattlePvETurtleLv1 battleHandler;

    [Header("Scene")]
    public string mapSceneName;

    [Header("UI")]
    public Canvas loadingCanvas;

    private bool hasStarted = false;

    private void Update()
    {
        if (!hasStarted && Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(BattleSequence());
        }
    }

    IEnumerator BattleSequence()
    {
        hasStarted = true;

        Debug.Log("BattleController: Start Battle");

        // 1️⃣ show loading
        if (loadingCanvas != null)
            loadingCanvas.gameObject.SetActive(true);

        // 2️⃣ load map
        AsyncOperation loadOp =
            SceneManager.LoadSceneAsync(mapSceneName, LoadSceneMode.Additive);

        if (loadOp == null)
        {
            Debug.LogError("Scene load failed. Scene not found in Build Settings.");
            yield break;
        }

        while (!loadOp.isDone)
        {
            yield return null;
        }

        Debug.Log("Map Loaded");

        // 3️⃣ hide loading
        if (loadingCanvas != null)
            loadingCanvas.gameObject.SetActive(false);

        // 4️⃣ start battle
        if (battleHandler != null)
        {
            battleHandler.OnBattleEnded += OnBattleFinished;
            battleHandler.BeginBattle(this);
        }
    }

    // được gọi khi battle kết thúc
    public void OnBattleFinished()
    {
        Debug.Log("Battle Finished");

        if (loadingCanvas != null)
            loadingCanvas.gameObject.SetActive(true);

        Debug.Log("room load");
    }
}