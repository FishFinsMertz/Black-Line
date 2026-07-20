using UnityEngine;

public class WorldAudio : MonoBehaviour
{
    [SerializeField] private AudioSource worldAudioSource;
    [SerializeField] private AudioClip worldAudioClip;
    [SerializeField] private bool isLooping = true;
    [SerializeField] private float volumeScale = 1f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private bool playOnStart = false;

    private bool isPlaying = false;

    private void Start()
    {
        if (worldAudioSource == null)
            worldAudioSource = gameObject.AddComponent<AudioSource>();

        if (playOnStart)
            Play();
    }

    public void Play()
    {
        Play(fadeInDuration);
    }

    public void Play(float fadeDuration)
    {
        if (isPlaying) return;
        if (worldAudioClip == null || AudioManager.Instance == null) return;

        isPlaying = true;

        if (isLooping)
        {
            AudioManager.Instance.ConfigureLoop(
                worldAudioSource,
                worldAudioClip,
                volumeScale: volumeScale,
                fadeInDuration: fadeDuration,
                spatialBlend: 1f,
                minDistance: 1f,
                maxDistance: 100f
            );
        }
        else
        {
            worldAudioSource.volume = 0f;
            worldAudioSource.PlayOneShot(worldAudioClip, volumeScale);
            isPlaying = false;
        }
    }

    public void Stop()
    {
        Stop(fadeOutDuration);
    }

    public void Stop(float fadeDuration)
    {
        if (!isPlaying) return;

        if (isLooping)
        {
            if (worldAudioSource != null && worldAudioSource.isPlaying)
            {
                AudioManager.Instance.FadeOut(worldAudioSource, fadeDuration);
            }
        }
        else
        {
            if (worldAudioSource != null && worldAudioSource.isPlaying)
                worldAudioSource.Stop();
        }

        isPlaying = false;
    }

    public void Toggle()
    {
        if (isPlaying)
            Stop();
        else
            Play();
    }

    public void SetVolume(float volume)
    {
        volumeScale = Mathf.Clamp01(volume);
        if (worldAudioSource != null && isLooping)
            worldAudioSource.volume = volumeScale;
    }

    public bool IsPlaying() => isPlaying;

    private void OnDisable()
    {
        if (isLooping && worldAudioSource != null && worldAudioSource.isPlaying)
        {
            worldAudioSource.Stop();
            isPlaying = false;
        }
    }
}