using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleThermal : MonoBehaviour
{
    private ParticleSystem ps;
    private Material uniqueMaterial;
    private Material uniqueTrailMaterial;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null) return;

        ParticleSystemRenderer psRenderer = ps.GetComponent<ParticleSystemRenderer>();
        if (psRenderer == null) return;

        // Unique instance for particle material
        uniqueMaterial = new Material(psRenderer.material);
        psRenderer.material = uniqueMaterial;
        uniqueMaterial.DisableKeyword("THERMAL_ON");

        // Unique instance for trail material if one exists
        if (psRenderer.trailMaterial != null)
        {
            uniqueTrailMaterial = new Material(psRenderer.trailMaterial);
            psRenderer.trailMaterial = uniqueTrailMaterial;
            uniqueTrailMaterial.DisableKeyword("THERMAL_ON");
        }

        if (ThermalManager.Instance != null)
            OnThermalToggled(ThermalManager.Instance.IsThermalEnabled());
        else
            OnThermalToggled(false);
    }

    private void OnEnable()
    {
        ThermalManager.OnThermalToggled += OnThermalToggled;
    }

    private void OnDisable()
    {
        ThermalManager.OnThermalToggled -= OnThermalToggled;
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
}