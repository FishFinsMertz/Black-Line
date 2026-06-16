using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleThermal : BaseThermalComponent
{
    [Header("Thermal Settings")]
    [SerializeField, Range(0f, 100f)] private float temperature = 0f;

    private ParticleSystem ps;
    private Material uniqueMaterial;
    private Material uniqueTrailMaterial;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null) return;

        ParticleSystemRenderer psRenderer = ps.GetComponent<ParticleSystemRenderer>();
        if (psRenderer == null) return;

        uniqueMaterial = new Material(psRenderer.material);
        psRenderer.material = uniqueMaterial;
        uniqueMaterial.DisableKeyword("THERMAL_ON");

        if (psRenderer.trailMaterial != null)
        {
            uniqueTrailMaterial = new Material(psRenderer.trailMaterial);
            psRenderer.trailMaterial = uniqueTrailMaterial;
            uniqueTrailMaterial.DisableKeyword("THERMAL_ON");
        }
    }

    private void Start()
    {
        ApplyTemperature();
    }

    private void OnEnable()
    {
        ApplyTemperature();
        SubscribeToThermalEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromThermalEvents();
    }

    private void ApplyTemperature()
    {
        if (uniqueMaterial != null)
            uniqueMaterial.SetFloat(TemperatureProperty, temperature);

        if (uniqueTrailMaterial != null)
            uniqueTrailMaterial.SetFloat(TemperatureProperty, temperature);
    }

    protected override void OnThermalToggled(bool enabled)
    {
        SetThermalKeyword(uniqueMaterial, enabled);
        SetThermalKeyword(uniqueTrailMaterial, enabled);
    }

    public void SetTemperature(float newTemperature)
    {
        temperature = Mathf.Clamp(newTemperature, 0f, 100f);
        ApplyTemperature();
    }

    public void AddTemperature(float delta)
    {
        temperature = Mathf.Clamp(temperature + delta, 0f, 100f);
        ApplyTemperature();
    }

    public float GetTemperature() => temperature;
}