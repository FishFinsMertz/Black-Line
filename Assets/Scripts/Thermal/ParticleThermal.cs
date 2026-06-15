using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleThermal : MonoBehaviour
{
    [Header("Thermal Settings")]
    [SerializeField, Range(0f, 100f)] private float temperature = 0f;

    private static readonly int TemperatureProperty = Shader.PropertyToID("_Temperature");

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
        ThermalManager.OnThermalToggled += OnThermalToggled;
        ApplyTemperature();
        if (ThermalManager.Instance != null)
            OnThermalToggled(ThermalManager.Instance.IsThermalEnabled());
        else
            OnThermalToggled(false);
    }

    private void OnDisable()
    {
        ThermalManager.OnThermalToggled -= OnThermalToggled;
    }

    private void ApplyTemperature()
    {
        if (uniqueMaterial != null)
            uniqueMaterial.SetFloat(TemperatureProperty, temperature);

        if (uniqueTrailMaterial != null)
            uniqueTrailMaterial.SetFloat(TemperatureProperty, temperature);
    }

    private void OnThermalToggled(bool enabled)
    {
        if (uniqueMaterial != null)
        {
            if (enabled) uniqueMaterial.EnableKeyword("THERMAL_ON");
            else uniqueMaterial.DisableKeyword("THERMAL_ON");
        }

        if (uniqueTrailMaterial != null)
        {
            if (enabled) uniqueTrailMaterial.EnableKeyword("THERMAL_ON");
            else uniqueTrailMaterial.DisableKeyword("THERMAL_ON");
        }
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