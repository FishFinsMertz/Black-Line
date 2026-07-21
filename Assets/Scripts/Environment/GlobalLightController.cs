using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLightController : MonoBehaviour
{
    [Header("Light Reference")]
    [SerializeField] private Light2D globalLight;

    [Header("Darkness Settings")]
    [SerializeField] private Color darknessColor = new Color(0.05f, 0.05f, 0.05f);

    private Color defaultColor;
    private float defaultIntensity;

    private void Start()
    {
        if (globalLight == null)
            globalLight = GetComponent<Light2D>();

        if (globalLight == null)
        {
            //Debug.LogWarning("GlobalLightController: No Light2D found. Searching scene...");
            globalLight = FindFirstObjectByType<Light2D>();
        }

        if (globalLight != null)
        {
            defaultColor = globalLight.color;
            defaultIntensity = globalLight.intensity;
        }
    }

    public void SetDarkness()
    {
        if (globalLight == null) return;
        globalLight.color = darknessColor;
    }

    public void ResetToDefault()
    {
        if (globalLight == null) return;
        globalLight.color = defaultColor;
        globalLight.intensity = defaultIntensity;
    }

    public void SetIntensity(float intensity)
    {
        if (globalLight == null) return;
        globalLight.intensity = Mathf.Max(0f, intensity);
    }
}