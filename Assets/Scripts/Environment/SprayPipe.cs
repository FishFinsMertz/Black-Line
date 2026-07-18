using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class SprayPipe : MonoBehaviour, ISaveable
{
    [Header("Save ID (unique per component)")]
    [SerializeField] private string saveID;

    [Header("Spray Settings")]
    [SerializeField] private float activeEmissionRate = 50f;
    [SerializeField] private float inactiveEmissionRate = 0f;
    [SerializeField] private float activeLightIntensity = 2f;
    [SerializeField] private float inactiveLightIntensity = 0f;
    [SerializeField] private float activeFresnelRadius = 1f;
    [SerializeField] private float inactiveFresnelRadius = 0f;
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sprayAudioClip;
    [SerializeField] private float audioFadeInDuration = 0.5f;
    [SerializeField] private float audioFadeOutDuration = 0.5f;

    [Header("Heat Source (Optional)")]
    [SerializeField] private bool requireSource = false;
    [SerializeField] private List<TileMapThermal> sourceTilemaps = new List<TileMapThermal>();

    [Header("References")]
    [SerializeField] private ParticleSystem sprayParticle;
    [SerializeField] private Light2D sprayLight;
    [SerializeField] private List<TileMapThermal> tilemapThermals;
    [SerializeField] private List<ThermalObject> thermalObjects;

    [Header("State")]
    [SerializeField] private bool startActive = false;

    private bool isActive;
    private bool isSourceActive = false;
    private ParticleSystem.EmissionModule emissionModule;
    private float currentEmissionRate;
    private float currentLightIntensity;
    private float currentFresnelRadius;

    private void Awake()
    {
        if (sprayParticle != null)
            emissionModule = sprayParticle.emission;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        SaveManager.Instance?.Register(this);
        isActive = startActive;
        currentEmissionRate = isActive ? activeEmissionRate : inactiveEmissionRate;
        currentLightIntensity = isActive ? activeLightIntensity : inactiveLightIntensity;
        currentFresnelRadius = isActive ? activeFresnelRadius : inactiveFresnelRadius;

        ApplyStateInstant(isActive);
    }

    private void Update()
    {
        if (requireSource)
        {
            bool currentlyActive = CheckSourceActive();
            if (currentlyActive != isSourceActive)
            {
                isSourceActive = currentlyActive;
                ApplyStateInstant(isActive && isSourceActive);
            }
        }
    }

    private bool CheckSourceActive()
    {
        if (!requireSource || sourceTilemaps.Count == 0)
            return true;

        foreach (var tm in sourceTilemaps)
        {
            if (tm != null && tm.GetCurrentTemperature() > 50f)
                return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    public void Activate()
    {
        if (isActive) return;
        isActive = true;

        if (requireSource)
            isSourceActive = CheckSourceActive();

        if (isSourceActive || !requireSource)
            StartCoroutine(SmoothTransition(true));
        else
            Debug.Log($"SprayPipe '{name}' requires heat source but none is active.");
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        StartCoroutine(SmoothTransition(false));
    }

    public void Toggle()
    {
        if (isActive)
            Deactivate();
        else
            Activate();
    }

    private IEnumerator SmoothTransition(bool toActive)
    {
        bool canActivate = toActive && (!requireSource || isSourceActive);
        if (toActive && !canActivate)
        {
            yield break;
        }

        if (toActive && sprayAudioClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.ConfigureLoop(
                audioSource,
                sprayAudioClip,
                volumeScale: 0.5f,
                fadeInDuration: audioFadeInDuration,
                maxDistance: 30f
            );
        }
        else if (!toActive && audioSource != null && audioSource.isPlaying && AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOut(audioSource, audioFadeOutDuration);
        }

        float targetEmission = toActive ? activeEmissionRate : inactiveEmissionRate;
        float targetLight = toActive ? activeLightIntensity : inactiveLightIntensity;
        float targetRadius = toActive ? activeFresnelRadius : inactiveFresnelRadius;

        float startEmission = currentEmissionRate;
        float startLight = currentLightIntensity;
        float startRadius = currentFresnelRadius;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float lerpedEmission = Mathf.Lerp(startEmission, targetEmission, t);
            float lerpedLight = Mathf.Lerp(startLight, targetLight, t);
            float lerpedRadius = Mathf.Lerp(startRadius, targetRadius, t);

            if (sprayParticle != null)
                emissionModule.rateOverTime = lerpedEmission;
            if (sprayLight != null)
                sprayLight.intensity = lerpedLight;

            foreach (var tm in tilemapThermals)
                if (tm != null)
                    tm.SetFresnelRadius(lerpedRadius);

            foreach (var tobj in thermalObjects)
                if (tobj != null)
                    tobj.SetFresnelRadius(lerpedRadius);

            yield return null;
        }

        currentEmissionRate = targetEmission;
        currentLightIntensity = targetLight;
        currentFresnelRadius = targetRadius;

        if (sprayParticle != null)
            emissionModule.rateOverTime = targetEmission;
        if (sprayLight != null)
            sprayLight.intensity = targetLight;
        foreach (var tm in tilemapThermals)
            if (tm != null)
                tm.SetFresnelRadius(targetRadius);
        foreach (var tobj in thermalObjects)
            if (tobj != null)
                tobj.SetFresnelRadius(targetRadius);

        if (!toActive && audioSource != null && audioSource.isPlaying && AudioManager.Instance != null)
        {
            if (audioFadeOutDuration <= 0f)
            {
                audioSource.Stop();
            }
        }
    }

    private void ApplyStateInstant(bool active)
    {
        bool canActivate = active && (!requireSource || isSourceActive);

        float targetEmission = canActivate ? activeEmissionRate : inactiveEmissionRate;
        float targetLight = canActivate ? activeLightIntensity : inactiveLightIntensity;
        float targetRadius = canActivate ? activeFresnelRadius : inactiveFresnelRadius;

        currentEmissionRate = targetEmission;
        currentLightIntensity = targetLight;
        currentFresnelRadius = targetRadius;

        if (sprayParticle != null)
            emissionModule.rateOverTime = targetEmission;
        if (sprayLight != null)
            sprayLight.intensity = targetLight;
        foreach (var tm in tilemapThermals)
            if (tm != null)
                tm.SetFresnelRadius(targetRadius);
        foreach (var tobj in thermalObjects)
            if (tobj != null)
                tobj.SetFresnelRadius(targetRadius);

        if (AudioManager.Instance != null)
        {
            if (canActivate && sprayAudioClip != null)
            {
                AudioManager.Instance.ConfigureLoop(
                    audioSource,
                    sprayAudioClip,
                    volumeScale: 0.5f,
                    fadeInDuration: 0f,
                    maxDistance: 30f
                );
            }
            else
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
    }

    public bool IsActive() => isActive;

    public void Save(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;

        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = isActive ? "Active" : "Inactive" });
    }

    public void Load(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            isActive = cs.state == "Active";
            isSourceActive = CheckSourceActive();
            ApplyStateInstant(isActive && (isSourceActive || !requireSource));
        }
    }
}