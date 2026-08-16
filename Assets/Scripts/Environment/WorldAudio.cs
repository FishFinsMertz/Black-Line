using UnityEngine;
using System.Collections;

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

    [Header("Delays")]
    [SerializeField] private float playDelay = 0f;
    [SerializeField] private float stopDelay = 0f;

    private bool isPlaying = false;
    private Coroutine playCoroutine;
    private Coroutine stopCoroutine;
    private bool isPlayDelaying = false;
    private bool isStopDelaying = false;

    private void Start()
    {
        if (worldAudioSource == null)
            worldAudioSource = gameObject.AddComponent<AudioSource>();

        SaveManager.Instance?.Register(this);

        if (worldAudioClip != null)
            worldAudioClip.LoadAudioData();

        if (playOnStart)
            Play();
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
        if (playCoroutine != null) StopCoroutine(playCoroutine);
        if (stopCoroutine != null) StopCoroutine(stopCoroutine);
    }

    public void Play()
    {
        Play(fadeInDuration);
    }

    public void Play(float fadeDuration)
    {
        if (isPlaying || isPlayDelaying) return;
        if (worldAudioClip == null || AudioManager.Instance == null) return;

        if (isStopDelaying && stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
            stopCoroutine = null;
            isStopDelaying = false;
        }

        isPlayDelaying = true;
        playCoroutine = StartCoroutine(PlayDelayed(fadeDuration));
    }

    private IEnumerator PlayDelayed(float fadeDuration)
    {
        if (playDelay > 0f)
            yield return new WaitForSeconds(playDelay);

        isPlayDelaying = false;
        playCoroutine = null;

        if (worldAudioClip == null || AudioManager.Instance == null)
            yield break;

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
        if (!isPlaying && !isPlayDelaying) return;

        if (isPlayDelaying && playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
            isPlayDelaying = false;
            return;
        }

        if (isStopDelaying) return;

        isStopDelaying = true;
        stopCoroutine = StartCoroutine(StopDelayed(fadeDuration));
    }

    public void StopImmediate()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
            isPlayDelaying = false;
        }
        if (stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
            stopCoroutine = null;
            isStopDelaying = false;
        }

        if (worldAudioSource != null && worldAudioSource.isPlaying)
            worldAudioSource.Stop();

        isPlaying = false;
        isStopDelaying = false;
        isPlayDelaying = false;
    }

    private IEnumerator StopDelayed(float fadeDuration)
    {
        if (stopDelay > 0f)
            yield return new WaitForSeconds(stopDelay);

        isStopDelaying = false;
        stopCoroutine = null;

        if (!isPlaying) yield break;

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
        if (isPlaying || isPlayDelaying)
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
        }
        isPlaying = false;
        isPlayDelaying = false;
        isStopDelaying = false;
        if (playCoroutine != null) StopCoroutine(playCoroutine);
        if (stopCoroutine != null) StopCoroutine(stopCoroutine);
        playCoroutine = null;
        stopCoroutine = null;
    }

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

        if (isPlayDelaying || isStopDelaying)
        {
            if (playCoroutine != null) StopCoroutine(playCoroutine);
            if (stopCoroutine != null) StopCoroutine(stopCoroutine);
            playCoroutine = null;
            stopCoroutine = null;
            isPlayDelaying = false;
            isStopDelaying = false;
        }

        if (shouldBePlaying && !isPlaying)
        {
            Play();
        }
        else if (!shouldBePlaying && isPlaying)
        {
            StopImmediate();
        }
    }
}