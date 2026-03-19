using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class IntroBattlePVE : MonoBehaviour
{
    public System.Action OnIntroFinished;

    [Header("Camera Config")]
    [SerializeField] private CinemachineCamera introVcam;

    public void PlayIntro()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        Debug.Log("🎬 Intro: Director mode active.");
        if (introVcam != null) introVcam.Priority = 50;

        // Tìm tất cả Mesh có gắn script hiệu ứng
        SpawnEffects[] allEffects = FindObjectsByType<SpawnEffects>(FindObjectsSortMode.None);

        foreach (var effect in allEffects)
        {
            // 1. Lia Camera tới Mesh đang chuẩn bị diễn
            if (introVcam != null) introVcam.Target.TrackingTarget = effect.transform;

            // Đợi Cam lướt tới (Damping của Cinemachine)
            yield return new WaitForSeconds(1.0f);

            // 2. Ra lệnh cho Mesh tự diễn hiệu ứng của nó
            effect.PlayEffect();

            // Đợi diễn xong + một chút thời gian để người chơi nhìn rõ
            yield return new WaitForSeconds(1.5f);
        }

        Debug.Log("🎬 Intro: Finished. Passing control to BattleHandler.");
        if (introVcam != null) introVcam.Priority = 5;

        OnIntroFinished?.Invoke();
    }
}