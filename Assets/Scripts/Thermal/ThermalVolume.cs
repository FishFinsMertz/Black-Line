using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class ThermalVolume : MonoBehaviour
{
    // When thermal turned on, turn on thermal volume, turn off main volume, vice versa

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
        ThermalManager.OnThermalToggled += OnThermalToggled;
        if (ThermalManager.Instance != null)
            OnThermalToggled(ThermalManager.Instance.IsThermalEnabled());
        else
            OnThermalToggled(false);
    }

    private void OnDisable()
    {
        ThermalManager.OnThermalToggled -= OnThermalToggled;
    }

    private void OnThermalToggled(bool enabled)
    {
        if (thermalVolume != null)
            thermalVolume.enabled = enabled;
        if (mainVolume != null)
            mainVolume.enabled = !enabled;
    }
}
