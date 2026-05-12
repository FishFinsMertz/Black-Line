using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class ThermalVolume : MonoBehaviour
{
    private Volume volume;

    private void Awake()
    {
        volume = GetComponent<Volume>();
        volume.enabled = false;
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
        if (volume != null)
            volume.enabled = enabled;
    }
}
