using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleThermal : MonoBehaviour
{
    private ParticleSystem ps;
    private Material uniqueMaterial;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null) return;

        // Get the renderer of the particle system
        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer == null) return;

        // Create a unique material instance to avoid affecting other particles
        uniqueMaterial = new Material(renderer.material);
        renderer.material = uniqueMaterial;

        // Initially disable thermal keyword (will be enabled by ThermalManager if needed)
        uniqueMaterial.DisableKeyword("THERMAL_ON");

        // Subscribe to thermal toggle event and apply initial state
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
        if (uniqueMaterial == null) return;
        if (enabled)
            uniqueMaterial.EnableKeyword("THERMAL_ON");
        else
            uniqueMaterial.DisableKeyword("THERMAL_ON");
    }
}