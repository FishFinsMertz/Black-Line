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

    [Header("References")]
    [SerializeField] private ParticleSystem sprayParticle;
    [SerializeField] private Light2D sprayLight;
    [SerializeField] private List<TileMapThermal> tilemapThermals;

    [Header("State")]
    [SerializeField] private bool startActive = false;

    private bool isActive;
    private ParticleSystem.EmissionModule emissionModule;
    private float currentEmissionRate;
    private float currentLightIntensity;
    private float currentFresnelRadius;

    private void Awake()
    {
        if (sprayParticle != null)
            emissionModule = sprayParticle.emission;
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

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    // Public API
    public void Activate()
    {
        if (isActive) return;
        isActive = true;
        StartCoroutine(SmoothTransition(true));
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
    }

    private void ApplyStateInstant(bool active)
    {
        float targetEmission = active ? activeEmissionRate : inactiveEmissionRate;
        float targetLight = active ? activeLightIntensity : inactiveLightIntensity;
        float targetRadius = active ? activeFresnelRadius : inactiveFresnelRadius;

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
    }

    public bool IsActive() => isActive;

    // --- ISaveable ---
    public void Save(GameData data)
    {
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
            // Apply the loaded state instantly (no transition)
            ApplyStateInstant(isActive);
        }
    }
}