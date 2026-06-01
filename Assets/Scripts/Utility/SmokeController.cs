using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SmokeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ThermalObject thermalSource;

    [Header("Temperature Thresholds")]
    [SerializeField] private float minTemp = 70f;
    [SerializeField] private float maxTemp = 100f;

    [Header("Emission Range (particles per second)")]
    [SerializeField] private float minEmission = 0f;
    [SerializeField] private float maxEmission = 30f;

    [Header("Death Linger")]
    [SerializeField] private float lingerDuration = 2f;

    private ParticleSystem smokeSystem;
    private ParticleSystem.EmissionModule emissionModule;
    private bool isDying = false;

    private void Start()
    {
        smokeSystem = GetComponent<ParticleSystem>();
        emissionModule = smokeSystem.emission;

        if (thermalSource == null)
            thermalSource = GetComponentInParent<ThermalObject>();

        if (thermalSource == null)
            Debug.LogWarning("SmokeController: No ThermalObject found. Smoke won't scale.", this);
    }

    private void Update()
    {
        if (isDying) return;
        if (thermalSource == null) return;

        float temp = thermalSource.GetCurrentTemperature();
        float t = (temp > minTemp) ? Mathf.Clamp01((temp - minTemp) / (maxTemp - minTemp)) : 0f;
        emissionModule.rateOverTime = Mathf.Lerp(minEmission, maxEmission, t);
    }

    public void OnOwnerDied()
    {
        isDying = true;

        // Detach now so the parent destruction doesn't take us with it
        transform.SetParent(null);

        emissionModule.rateOverTime = 0f;
        smokeSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);

        Destroy(gameObject, lingerDuration);
    }
}