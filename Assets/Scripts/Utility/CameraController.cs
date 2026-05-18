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
    [SerializeField] private float maxTiltAngle = 5f;           
    [SerializeField] private float tiltFrequency = 3f;          
    [SerializeField] private float tiltLerpSpeed = 3f;
    
    [Header("Temperature Threshold")]
    [SerializeField] private float wobbleStartTemp = 30f;
    [SerializeField] private float wobbleMaxAtTemp = 0f;
    [SerializeField] private float wobblePower = 2f;

    private ThermalObject playerThermal;

    private Transform target;      
    private string defaultTargetTag = "Player"; 
    private float currentWobbleIntensity = 0f;
    private float currentTiltIntensity = 0f;
    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.rotation;
        FindAndAssignPlayer();
    }

    private void FindAndAssignPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(defaultTargetTag);
        if (playerObj == null)
        {
            Debug.LogWarning($"No GameObject with tag '{defaultTargetTag}' found.");
            return;
        }

        target = playerObj.transform;  // camera follows the Player root

        Transform bodyTransform = playerObj.transform.Find("Body");
        if (bodyTransform != null)
            playerThermal = bodyTransform.GetComponent<ThermalObject>();
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
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
        
        if (enableWobble)
            currentWobbleIntensity = Mathf.Lerp(currentWobbleIntensity, targetIntensity * maxWobbleIntensity, wobbleLerpSpeed * Time.deltaTime);
        if (enableTilt)
            currentTiltIntensity = Mathf.Lerp(currentTiltIntensity, targetIntensity * maxTiltAngle, tiltLerpSpeed * Time.deltaTime);
        
        if (currentWobbleIntensity > 0.001f)
        {
            float time = Time.time * wobbleFrequency;
            float wobbleX = Mathf.Sin(time) * currentWobbleIntensity;
            float wobbleY = Mathf.Cos(time * 1.3f) * currentWobbleIntensity;
            desiredPosition += new Vector3(wobbleX, wobbleY, 0f);
            smoothedPos = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        
        Quaternion targetRotation = initialRotation;
        if (currentTiltIntensity > 0.001f)
        {
            float tiltTime = Time.time * tiltFrequency;
            float tiltZ = Mathf.Sin(tiltTime) * currentTiltIntensity;
            tiltZ += Mathf.Sin(tiltTime * 2.3f) * (currentTiltIntensity * 0.5f);
            targetRotation = initialRotation * Quaternion.Euler(0f, 0f, tiltZ);
        }
        
        transform.position = smoothedPos;
        transform.rotation = targetRotation;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (newTarget != null)
        {
            // Attempt to find ThermalObject in the same way
            playerThermal = newTarget.GetComponentInChildren<ThermalObject>();
            if (playerThermal == null)
                playerThermal = newTarget.GetComponent<ThermalObject>();
        }
    }
    
    public void ResetToPlayer()
    {
        FindAndAssignPlayer();
    }
}