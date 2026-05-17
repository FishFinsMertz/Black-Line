using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class ColdEffectsController : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private ThermalObject playerThermal;

    [Header("Temperature Mapping")]
    [SerializeField] private float maxColdWeight = 1f;
    [SerializeField] private float minColdWeight = 0f; 
    [SerializeField] private float warmThreshold = 70f;  

    private Volume coldVolume;

    private void Start()
    {
        coldVolume = GetComponent<Volume>();
        if (playerThermal == null)
            playerThermal = GetComponentInParent<ThermalObject>();
    }

    private void Update()
    {
        if (playerThermal == null) return;

        float temp = playerThermal.GetTemperature();
        float weight = 0f;

        if (temp < warmThreshold)
        {
            weight = 1f - (temp / warmThreshold);
            weight = Mathf.Clamp01(weight);
            weight = weight * maxColdWeight;
        }

        // Smooth transition
        coldVolume.weight = Mathf.Lerp(coldVolume.weight, weight, 10f * Time.deltaTime);
    }
}