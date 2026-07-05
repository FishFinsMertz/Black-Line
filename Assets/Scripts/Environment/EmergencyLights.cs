using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for Light2D

public class EmergencyLights : MonoBehaviour
{
    [Header("Light References")]
    [SerializeField] private Light2D[] emergencyLights; // Optional – auto-fills if empty

    [Header("Pulse Settings")]
    [SerializeField] private float minIntensity = 0.1f;
    [SerializeField] private float maxIntensity = 1.0f;
    [SerializeField] private float pulseSpeed = 1.0f;
    [SerializeField] private float phaseOffset = 0f;

    private void Start()
    {
        if (emergencyLights == null || emergencyLights.Length == 0)
        {
            emergencyLights = GetComponentsInChildren<Light2D>();
            if (emergencyLights.Length == 0)
                Debug.LogWarning($"{name}: No Light2D components found in children.");
        }
    }

    private void Update()
    {
        if (emergencyLights == null || emergencyLights.Length == 0) return;

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