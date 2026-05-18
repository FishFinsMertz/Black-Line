using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

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

    private void OnEnable()
    {
        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re‑find the player after every scene load
        FindPlayerThermal();
        // Also reset volume weight to avoid sudden jumps (optional)
        // coldVolume.weight = 0;
    }

    private void FindPlayerThermal()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("No GameObject with tag 'Player' found. Cold effects won't work.");
            playerThermal = null;
            return;
        }

        Transform bodyTransform = playerObj.transform.Find("Body");
        if (bodyTransform != null)
            playerThermal = bodyTransform.GetComponent<ThermalObject>();
        else
            playerThermal = playerObj.GetComponent<ThermalObject>();
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
        coldVolume.weight = weight;
    }
}