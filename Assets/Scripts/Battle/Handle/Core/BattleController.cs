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
    public LoadingUI loadingUI;

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
        loadingUI.Show();

        // 2️⃣ load map
        AsyncOperation loadOp =
            SceneManager.LoadSceneAsync(mapSceneName, LoadSceneMode.Additive);

        while (!loadOp.isDone)
        {
            yield return null;
        }

        Debug.Log("Map Loaded");

        // 3️⃣ fake loading
        yield return StartCoroutine(loadingUI.FakeLoading());

        // 4️⃣ hide loading
        loadingUI.Hide();

        // 5️⃣ start battle
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

        loadingUI.Show();

        Debug.Log("room load");
    }
}