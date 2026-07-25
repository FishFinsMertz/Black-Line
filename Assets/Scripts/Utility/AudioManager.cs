using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup uiGroup;

    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float uiVolume = 1f;

    private Dictionary<AudioSource, Coroutine> activeFades = new Dictionary<AudioSource, Coroutine>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyAllVolumes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) SetMasterVolume(0f);
        else if (Input.GetKeyDown(KeyCode.O)) SetMasterVolume(0.5f);
        else if (Input.GetKeyDown(KeyCode.P)) SetMasterVolume(1f);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyAllVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolume("SFXVolume", sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolume("MusicVolume", musicVolume);
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        ApplyVolume("UIVolume", uiVolume);
    }

    private void ApplyAllVolumes()
    {
        ApplyVolume("SFXVolume", masterVolume * sfxVolume);
        ApplyVolume("MusicVolume", masterVolume * musicVolume);
        ApplyVolume("UIVolume", masterVolume * uiVolume);
    }

    private void ApplyVolume(string parameter, float volume)
    {
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        audioMixer.SetFloat(parameter, dB);
    }

    private void CancelActiveFade(AudioSource source)
    {
        if (source == null) return;
        if (activeFades.TryGetValue(source, out Coroutine coroutine))
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            activeFades.Remove(source);
        }
    }

    public void PlayOneShot(
        AudioClip clip,
        Vector3 position,
        AudioMixerGroup group = null,
        float volumeScale = 1f,
        float pitchVariation = 0f,
        float maxDistance = 60f)
    {
        if (clip == null) return;
        GameObject go = new GameObject("OneShot3D");
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volumeScale);
        source.spatialBlend = 1f;
        source.transform.position = position;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.outputAudioMixerGroup = group ?? sfxGroup;
        if (pitchVariation > 0f)
            source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.Play();
        Destroy(go, clip.length);
    }

    public void PlayOneShot2D(
        AudioClip clip,
        AudioMixerGroup group = null,
        float volumeScale = 1f,
        float pitchVariation = 0f)
    {
        if (clip == null) return;
        GameObject go = new GameObject("OneShot2D");
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volumeScale);
        source.spatialBlend = 0f;
        source.outputAudioMixerGroup = group ?? sfxGroup;
        if (pitchVariation > 0f)
            source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.Play();
        Destroy(go, clip.length);
    }

    public AudioSource PlayLoop(
        AudioClip clip,
        Vector3 position,
        AudioMixerGroup group = null,
        float volumeScale = 1f,
        float pitchVariation = 0f,
        float fadeInDuration = 0f,
        float spatialBlend = 1f,
        float minDistance = 1f,
        float maxDistance = 60f)
    {
        if (clip == null) return null;

        GameObject go = new GameObject("LoopSource");
        go.transform.position = position;
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.volume = 0f;
        source.spatialBlend = spatialBlend;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.outputAudioMixerGroup = group ?? sfxGroup;
        if (pitchVariation > 0f)
            source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

        source.Play();

        if (fadeInDuration > 0f)
        {
            CancelActiveFade(source);
            Coroutine c = StartCoroutine(FadeIn(source, fadeInDuration, Mathf.Clamp01(volumeScale)));
            activeFades[source] = c;
        }
        else
            source.volume = Mathf.Clamp01(volumeScale);

        return source;
    }

    public void StopLoop(AudioSource source, float fadeOutDuration = 0f)
    {
        if (source == null) return;
        if (fadeOutDuration > 0f && source.isPlaying)
        {
            CancelActiveFade(source);
            Coroutine c = StartCoroutine(FadeOutAndDestroy(source, fadeOutDuration));
            activeFades[source] = c;
        }
        else
        {
            CancelActiveFade(source);
            source.Stop();
            Destroy(source.gameObject);
        }
    }

    public void ConfigureLoop(
        AudioSource source,
        AudioClip clip,
        AudioMixerGroup group = null,
        float volumeScale = 1f,
        float pitchVariation = 0f,
        float fadeInDuration = 0f,
        float spatialBlend = 1f,
        float minDistance = 1f,
        float maxDistance = 60f)
    {
        if (source == null || clip == null) return;

        CancelActiveFade(source);

        source.clip = clip;
        source.loop = true;
        source.volume = 0f;
        source.spatialBlend = spatialBlend;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.outputAudioMixerGroup = group ?? sfxGroup;
        if (pitchVariation > 0f)
            source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

        source.Play();

        if (fadeInDuration > 0f)
        {
            Coroutine c = StartCoroutine(FadeIn(source, fadeInDuration, Mathf.Clamp01(volumeScale)));
            activeFades[source] = c;
        }
        else
            source.volume = Mathf.Clamp01(volumeScale);
    }

    public void FadeOut(AudioSource source, float duration)
    {
        if (source == null || !source.isPlaying) return;
        CancelActiveFade(source);
        Coroutine c = StartCoroutine(FadeOutRoutine(source, duration));
        activeFades[source] = c;
    }

    private IEnumerator FadeIn(AudioSource source, float duration, float targetVolume)
    {
        float elapsed = 0f;
        float startVolume = source.volume;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        source.volume = targetVolume;
        activeFades.Remove(source);
    }

    private IEnumerator FadeOutRoutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        source.volume = 0f;
        source.Stop();
        activeFades.Remove(source);
    }

    private IEnumerator FadeOutAndDestroy(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        source.Stop();
        activeFades.Remove(source);
        Destroy(source.gameObject);
    }
}