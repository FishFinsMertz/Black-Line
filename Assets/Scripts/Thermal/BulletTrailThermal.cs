using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class BulletTrailThermal : MonoBehaviour
{
    private TrailRenderer trail;
    private Material uniqueMaterial;

    private void Start()
    {
        trail = GetComponent<TrailRenderer>();
        
        uniqueMaterial = new Material(trail.material);
        trail.material = uniqueMaterial;
        
        uniqueMaterial.DisableKeyword("THERMAL_ON");
        
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