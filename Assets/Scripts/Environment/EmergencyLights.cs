using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for Light2D

public class EmergencyLights : MonoBehaviour
{
    [Header("Light References")]
    [SerializeField] private Light2D[] emergencyLights;

    [Header("Pulse Settings")]
    [SerializeField] private float minIntensity = 0.1f;
    [SerializeField] private float maxIntensity = 1.0f;
    [SerializeField] private float pulseSpeed = 1.0f;
    [SerializeField] private float phaseOffset = 0f; 

    private void Update()
    {
        float t = Time.time * pulseSpeed * Mathf.PI * 2f + phaseOffset * Mathf.Deg2Rad;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(t) + 1f) * 0.5f);
        
        foreach (Light2D light in emergencyLights)
        {
            if (light != null)
            {
                light.intensity = intensity;
            }
        }
    }
}