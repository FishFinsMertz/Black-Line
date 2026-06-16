using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class ThermalVolume : BaseThermalComponent
{
    private Volume thermalVolume;
    [SerializeField] private Volume mainVolume;

    private void Awake()
    {
        thermalVolume = GetComponent<Volume>();
        thermalVolume.enabled = false;

        mainVolume.enabled = true;
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
        if (thermalVolume != null)
            thermalVolume.enabled = enabled;
        if (mainVolume != null)
            mainVolume.enabled = !enabled;
    }
}
