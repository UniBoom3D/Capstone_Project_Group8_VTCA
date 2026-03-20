using UnityEngine;
using UnityEngine.UI;

public class TimeAudio : MonoBehaviour
{
    [Header("References")]
    public Image dialImage;
    public AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip normalClip;
    public AudioClip criticalClip;
    public AudioClip timeoutClip; 

    [Header("Settings")]
    public float normalInterval = 1.0f;
    public float criticalInterval = 1.0f;

    private float nextTickTime;
    private bool isAudioActive = false;

    void Update()
    {
        if (!isAudioActive || dialImage == null || audioSource == null) return;

        float fillValue = dialImage.fillAmount;


        float currentInterval = (fillValue > 0.3f) ? normalInterval : criticalInterval;
        AudioClip currentClip = (fillValue > 0.3f) ? normalClip : criticalClip;

        if (Time.time >= nextTickTime)
        {
            if (currentClip != null) audioSource.PlayOneShot(currentClip);
            nextTickTime = Time.time + currentInterval;
        }
    }

    public void StartAudio()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        isAudioActive = true;
        nextTickTime = Time.time;
    }

    public void StopAudio()
    {
        isAudioActive = false;
    }

    public void PlayTimeoutAudio()
    {
        isAudioActive = false; // Dừng các tiếng tích tắc
        if (audioSource != null && timeoutClip != null)
        {
            audioSource.PlayOneShot(timeoutClip);
        }
    }
}