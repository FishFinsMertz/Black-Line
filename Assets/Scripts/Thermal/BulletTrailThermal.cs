using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class BulletTrailThermal : BaseThermalComponent
{
    private TrailRenderer trail;
    private Material uniqueMaterial;

    private void Start()
    {
        trail = GetComponent<TrailRenderer>();
        
        uniqueMaterial = new Material(trail.material);
        trail.material = uniqueMaterial;
        
        DisableThermalKeyword(uniqueMaterial);
        
        SubscribeToThermalEvents();
    }

    private void OnEnable()
    {
        SubscribeToThermalEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromThermalEvents();
    }

    protected override void OnThermalToggled(bool enabled)
    {
        SetThermalKeyword(uniqueMaterial, enabled);
    }
}