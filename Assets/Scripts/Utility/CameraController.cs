using UnityEngine;

public class CameraController : MonoBehaviour
{       
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;     
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); 

    [Header("Positional Wobble (Shake)")]
    [SerializeField] private bool enableWobble = true;
    [SerializeField] private float maxWobbleIntensity = 0.15f;
    [SerializeField] private float wobbleFrequency = 8f;
    [SerializeField] private float wobbleLerpSpeed = 3f;
    
    [Header("Rotational Wobble (Drunk Tilt)")]
    [SerializeField] private bool enableTilt = true;
    [SerializeField] private float maxTiltAngle = 5f;           // maximum tilt in degrees
    [SerializeField] private float tiltFrequency = 3f;          // slower rotation than wobble
    [SerializeField] private float tiltLerpSpeed = 3f;
    
    [Header("Temperature Threshold")]
    [SerializeField] private float wobbleStartTemp = 30f;
    [SerializeField] private float wobbleMaxAtTemp = 0f;
    [SerializeField] private float wobblePower = 2f;

    [Header("Temperature Reference")]
    [SerializeField] private ThermalObject playerThermal;

    private Transform target;      
    private string defaultTargetTag = "Player"; 
    private float currentWobbleIntensity = 0f;
    private float currentTiltIntensity = 0f;
    private Quaternion initialRotation;

    private void Start()
    {
        // Store camera's original rotation
        initialRotation = transform.rotation;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(defaultTargetTag);
            if (player != null)
            {
                target = player.transform;
                if (playerThermal == null)
                    playerThermal = player.GetComponent<ThermalObject>();
            }
            else
                Debug.LogWarning($"No object with tag '{defaultTargetTag}' found.");
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        // Base follow (smooth position)
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Calculate intensity based on temperature (same for both wobbles)
        float targetIntensity = 0f;
        if (playerThermal != null)
        {
            float temp = playerThermal.GetTemperature();
            if (temp < wobbleStartTemp)
            {
                float t = (wobbleStartTemp - temp) / (wobbleStartTemp - wobbleMaxAtTemp);
                t = Mathf.Clamp01(t);
                t = Mathf.Pow(t, wobblePower);
                targetIntensity = t;
            }
        }
        
        // Smooth intensity updates
        if (enableWobble)
            currentWobbleIntensity = Mathf.Lerp(currentWobbleIntensity, targetIntensity * maxWobbleIntensity, wobbleLerpSpeed * Time.deltaTime);
        if (enableTilt)
            currentTiltIntensity = Mathf.Lerp(currentTiltIntensity, targetIntensity * maxTiltAngle, tiltLerpSpeed * Time.deltaTime);
        
        // Apply positional wobble (shake)
        if (currentWobbleIntensity > 0.001f)
        {
            float time = Time.time * wobbleFrequency;
            float wobbleX = Mathf.Sin(time) * currentWobbleIntensity;
            float wobbleY = Mathf.Cos(time * 1.3f) * currentWobbleIntensity;
            desiredPosition += new Vector3(wobbleX, wobbleY, 0f);
            smoothedPos = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        
        // Apply rotational wobble (drunk tilt)
        Quaternion targetRotation = initialRotation;
        if (currentTiltIntensity > 0.001f)
        {
            float tiltTime = Time.time * tiltFrequency;
            float tiltZ = Mathf.Sin(tiltTime) * currentTiltIntensity;
            // Also add a second harmonic for more erratic feel
            tiltZ += Mathf.Sin(tiltTime * 2.3f) * (currentTiltIntensity * 0.5f);
            targetRotation = initialRotation * Quaternion.Euler(0f, 0f, tiltZ);
        }
        
        // Apply position and rotation
        transform.position = smoothedPos;
        transform.rotation = targetRotation;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (newTarget != null)
            playerThermal = newTarget.GetComponent<ThermalObject>();
    }
    
    public void ResetToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(defaultTargetTag);
        if (player != null)
        {
            target = player.transform;
            if (playerThermal == null)
                playerThermal = player.GetComponent<ThermalObject>();
        }
    }
}