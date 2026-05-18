using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class ColdEffectsController : MonoBehaviour
{
    [Header("Temperature Mapping")]
    [SerializeField] private float maxColdWeight = 1f;
    [SerializeField] private float minColdWeight = 0f;
    [SerializeField] private float warmThreshold = 70f;

    private Volume coldVolume;
    private ThermalObject playerThermal;

    private void Start()
    {
        coldVolume = GetComponent<Volume>();
        FindPlayerThermal();
    }

    private void FindPlayerThermal()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("No GameObject with tag 'Player' found. Cold effects won't work.");
            return;
        }

        // Try to find ThermalObject on the "Body" child first
        Transform bodyTransform = playerObj.transform.Find("Body");
        if (bodyTransform != null)
            //Debug.Log("Found 'Body' child. Looking for ThermalObject there.");
            playerThermal = bodyTransform.GetComponent<ThermalObject>();
    }

    private void Update()
    {
        if (playerThermal == null) return;

        float temp = playerThermal.GetTemperature();
        float weight = 0f;

        if (temp < warmThreshold)
        {
            // weight = 1 at temp=0, weight = 0 at temp=warmThreshold
            weight = 1f - (temp / warmThreshold);
            weight = Mathf.Clamp01(weight);
            weight = weight * maxColdWeight;
        }

        // Smooth transition
        coldVolume.weight = Mathf.Lerp(coldVolume.weight, weight, 10f * Time.deltaTime);
    }
}