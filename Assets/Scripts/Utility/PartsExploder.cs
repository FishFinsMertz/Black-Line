using UnityEngine;

public class PartsExploder : MonoBehaviour
{
    [Header("Piece Settings")]
    public GameObject[] piecePrefabs;
    public Vector2 pieceForceMin = new Vector2(-1.5f, 0.5f);
    public Vector2 pieceForceMax = new Vector2(1.5f, 2.5f);
    public float pieceTorqueMin = -60f;
    public float pieceTorqueMax = 60f;

    [Header("Gore VFX")]
    public bool enableBloodSplatter = true;
    public ParticleSystem bloodBurstPrefab;

    [Header("Heat Dissipation")]
    public bool enableHeatDissipation = true;

    [Header("Camera Shake")]
    public float shakePower = 2f;

    private CameraController camController;
    private ThermalObject currentThermal;

    private void Start()
    {
        camController = Camera.main.GetComponent<CameraController>();
        currentThermal = GetComponentInChildren<ThermalObject>();
    }

    public void Explode()
    {
        float currentTemp = 50f;
        if (enableHeatDissipation && currentThermal != null)
            currentTemp = currentThermal.GetTemperature();

        if (enableBloodSplatter && bloodBurstPrefab != null)
        {
            if (camController != null)
                camController.TriggerShake(shakePower, 0.5f, 1f);
            ParticleSystem blood = Instantiate(bloodBurstPrefab, transform.position, Quaternion.identity);
            Destroy(blood.gameObject, blood.main.duration);
        }

        foreach (GameObject piecePrefab in piecePrefabs)
        {
            GameObject piece = Instantiate(piecePrefab, transform.position, Quaternion.identity);

            // Apply current temperature to all thermal objects on the piece if heat dissipation is enabled
            if (enableHeatDissipation)
            {
                foreach (ThermalObject thermal in piece.GetComponentsInChildren<ThermalObject>())
                {
                    thermal.SetCurrentTemperature(currentTemp);
                }
            }

            Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 force = new Vector2(
                    Random.Range(pieceForceMin.x, pieceForceMax.x),
                    Random.Range(pieceForceMin.y, pieceForceMax.y)
                );
                rb.AddForce(force, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(pieceTorqueMin, pieceTorqueMax));
            }
        }

        gameObject.SetActive(false);
    }
}