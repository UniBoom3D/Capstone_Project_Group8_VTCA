using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnEffects : MonoBehaviour
{
    // Danh sách các loại hiệu ứng xuất hiện
    public enum EffectType { ScaleUp, FadeIn, DropDown }
    [SerializeField] private EffectType currentEffect = EffectType.ScaleUp;
    [SerializeField] private float duration = 2f;

    private Renderer[] renderers;

    private void Awake()
    {
        // Tự động lấy tất cả renderer của Mesh này
        renderers = GetComponentsInChildren<Renderer>();
    }

    // Hàm thực hiện hiệu ứng dựa trên lựa chọn trong Inspector
    public void PlayEffect()
    {
        switch (currentEffect)
        {
            case EffectType.ScaleUp:
                StartCoroutine(ScaleRoutine());
                break;
                // Bạn có thể thêm các case khác cho Enemy sau này tại đây
        }
    }

    private IEnumerator ScaleRoutine()
    {
        // 1. Chuẩn bị: Ẩn và đưa về 0
        transform.localScale = Vector3.zero;
        foreach (var r in renderers) r.enabled = true;

        // 2. Diễn hoạt
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / duration);
            yield return null;
        }

        transform.localScale = Vector3.one;
        Debug.Log($"✨ Effect {currentEffect} finished on {gameObject.name}");
    }
}