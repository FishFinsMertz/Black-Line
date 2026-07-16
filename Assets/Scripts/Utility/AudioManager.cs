using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

    public System.Action<float> OnMasterVolumeChanged;

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
        ApplyMasterVolume();
    }

    public void SetMasterVolume(float volume)
    {
        Debug.Log($"SetMasterVolume called with {volume}");
        masterVolume = Mathf.Clamp01(volume);
        ApplyMasterVolume();
    }

    public float GetMasterVolume() => masterVolume;

    private void ApplyMasterVolume()
    {
        OnMasterVolumeChanged?.Invoke(masterVolume);
    }

    public void PlayOneShot(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        float finalVolume = masterVolume * volumeScale;
        AudioSource.PlayClipAtPoint(clip, position, finalVolume);
    }

    public void PlayOneShot2D(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        GameObject go = new GameObject("OneShot2D");
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(masterVolume * volumeScale);
        source.spatialBlend = 0f;
        source.Play();
        Destroy(go, clip.length);
    }
}