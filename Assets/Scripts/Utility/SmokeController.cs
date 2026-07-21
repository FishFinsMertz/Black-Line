using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SmokeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ThermalObject thermalSource;

    [Header("Hard Threshold")]
    [SerializeField] private float activationTemp = 100f;

    [Header("Emission Rate (when active)")]
    [SerializeField] private float activeEmission = 50f;

    [Header("Death Linger")]
    [SerializeField] private float lingerDuration = 2f;

    private ParticleSystem smokeSystem;
    private ParticleSystem.EmissionModule emissionModule;
    private bool isDying = false;

    private void Start()
    {
        smokeSystem = GetComponent<ParticleSystem>();
        emissionModule = smokeSystem.emission;
        emissionModule.rateOverTime = 0f;

        if (thermalSource == null)
            thermalSource = GetComponentInParent<ThermalObject>();
    }

    private void Update()
    {
        if (isDying) return;
        if (thermalSource == null) return;

        float temp = thermalSource.GetCurrentTemperature();
        float rate = (temp >= activationTemp) ? activeEmission : 0f;
        emissionModule.rateOverTime = rate;
    }

    public void OnOwnerDied()
    {
        if (isDying) return;
        isDying = true;

        transform.SetParent(null);

        emissionModule.rateOverTime = 0f;
        smokeSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);

        Destroy(gameObject, lingerDuration);
    }
}