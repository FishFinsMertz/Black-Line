using UnityEngine;

public class WorldAudio : MonoBehaviour, ISaveable
{
    [Header("Save ID (unique per component)")]
    [SerializeField] private string saveID;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource worldAudioSource;
    [SerializeField] private AudioClip worldAudioClip;
    [SerializeField] private bool isLooping = true;
    [SerializeField] private float volumeScale = 1f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private bool playOnStart = false;

    [Header("Spatial Settings")]
    [SerializeField] private bool is2D = false;
    [SerializeField] private float maxDistance = 100f;

    [Header("Doppler")]
    [SerializeField] private bool useDoppler = true;

    private bool isPlaying = false;

    private void Start()
    {
        if (worldAudioSource == null)
            worldAudioSource = gameObject.AddComponent<AudioSource>();

        SaveManager.Instance?.Register(this);

        if (playOnStart)
            Play();
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
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
                spatialBlend: is2D ? 0f : 1f,
                minDistance: 1f,
                maxDistance: maxDistance
            );

            worldAudioSource.dopplerLevel = useDoppler ? 1f : 0f;
        }
        else
        {
            worldAudioSource.dopplerLevel = useDoppler ? 1f : 0f;

            if (is2D)
            {
                AudioManager.Instance.PlayOneShot2D(worldAudioClip, volumeScale: volumeScale);
            }
            else
            {
                AudioManager.Instance.PlayOneShot(worldAudioClip, transform.position, volumeScale: volumeScale, maxDistance: maxDistance);
            }
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

    // --- ISaveable ---
    public void Save(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;

        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState
        {
            id = saveID,
            state = isPlaying ? "On" : "Off"
        });
    }

    public void Load(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;

        ComponentState cs = data.componentStates.Find(c => c.id == saveID);
        if (cs == null) return;

        bool shouldBePlaying = cs.state == "On";

        if (shouldBePlaying && !isPlaying)
        {
            Play();
        }
        else if (!shouldBePlaying && isPlaying)
        {
            Stop();
        }
    }
}