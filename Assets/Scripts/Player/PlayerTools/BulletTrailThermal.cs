using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class BulletTrailThermal : MonoBehaviour
{
    private TrailRenderer trail;
    private Material uniqueMaterial;

    private void Start()
    {
        trail = GetComponent<TrailRenderer>();
        
        // Create a unique material instance so toggling doesn't affect other bullets
        uniqueMaterial = new Material(trail.material);
        trail.material = uniqueMaterial;
        
        // Initially disable thermal keyword (will be enabled by ThermalManager if needed)
        uniqueMaterial.DisableKeyword("THERMAL_ON");
        
        // Subscribe to thermal toggle event
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