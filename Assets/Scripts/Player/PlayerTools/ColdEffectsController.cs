using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class ColdEffectsController : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private ThermalObject playerThermal; // player's ThermalObject

    [Header("Temperature Mapping")]
    [SerializeField] private float maxColdWeight = 1f;   // weight when temperature = 0
    [SerializeField] private float minColdWeight = 0f;   // weight when temperature >= warmThreshold
    [SerializeField] private float warmThreshold = 70f;  // above this temperature, no cold effects

    private Volume coldVolume;

    private void Start()
    {
        coldVolume = GetComponent<Volume>();
        if (playerThermal == null)
            playerThermal = GetComponentInParent<ThermalObject>(); // if script is child of player
    }

    private void Update()
    {
        if (playerThermal == null) return;

        float temp = playerThermal.GetTemperature(); // 0 = coldest, 100 = warmest
        float weight = 0f;

        if (temp < warmThreshold)
        {
            // Normalize: 0 at warmThreshold, 1 at 0
            weight = 1f - (temp / warmThreshold);
            weight = Mathf.Clamp01(weight);
            weight = weight * maxColdWeight;
        }

        // Smooth transition (optional)
        coldVolume.weight = Mathf.Lerp(coldVolume.weight, weight, 10f * Time.deltaTime);
    }
}