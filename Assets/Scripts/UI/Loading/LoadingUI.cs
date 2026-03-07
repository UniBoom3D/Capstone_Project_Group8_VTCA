using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingUI : MonoBehaviour
{
    public Canvas loadingCanvas;
    public Slider loadingSlider;

    public void Show()
    {
        loadingCanvas.enabled = true;
        loadingSlider.value = 0;
    }

    public void Hide()
    {
        loadingCanvas.enabled = false;
    }

    public IEnumerator FakeLoading()
    {
        float time = 0f;

        while (time < 5f)
        {
            time += Time.deltaTime;

            loadingSlider.value = time / 5f;

            yield return null;
        }

        loadingSlider.value = 1f;
    }
}