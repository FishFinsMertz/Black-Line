using UnityEngine;
using System.Collections;

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

    [Header("Random Delay")]
    [Tooltip("If > 0, the explosion will be delayed by a random time between min and max.")]
    public float delayMin = 0f;
    public float delayMax = 0f;

    private CameraController camController;
    private ThermalObject currentThermal;
    private bool isExploding = false;

    private void Start()
    {
        camController = Camera.main.GetComponent<CameraController>();
        currentThermal = GetComponentInChildren<ThermalObject>();
    }

    public void Explode()
    {
        if (isExploding) return;

        // If no delay, explode immediately
        if (delayMin <= 0f && delayMax <= 0f)
        {
            PerformExplosion();
        }
        else
        {
            isExploding = true;
            StartCoroutine(ExplodeWithDelay());
        }
    }

    private IEnumerator ExplodeWithDelay()
    {
        float delay = Random.Range(delayMin, delayMax);
        yield return new WaitForSeconds(delay);
        PerformExplosion();
        isExploding = false;
    }

    private void PerformExplosion()
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
        isExploding = false;
    }
}