using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;     
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -5f); 

    [Header("Look Ahead")]
    [SerializeField] private bool enableLookAhead = true;
    [SerializeField] private float lookAheadDistance = 3f;
    [SerializeField] private float lookAheadSpeed = 3f;
    [SerializeField] private float lookAheadReturnSpeed = 1f;

    [Header("Mouse Follow (Cursor) – Right Click Only")]
    [SerializeField] private bool enableMouseFollow = true;
    [SerializeField] private float minFollowDistance = 1f;
    [SerializeField] private float maxFollowDistance = 3f;
    [SerializeField] private float mouseFollowSmoothing = 8f;

    [Header("Positional Wobble (Temperature Based)")]
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

    [Header("Camera Shake (External)")]
    [SerializeField] private float shakeDecaySpeed = 5f;

    private GeneralThermalRegulator playerThermal;
    private Transform target;      
    private string defaultTargetTag = "Player"; 
    private float currentWobbleIntensity = 0f;
    private float currentTiltIntensity = 0f;
    private Quaternion initialRotation;

    private float currentShakeStrength = 0f;
    private float shakeRemainingTime = 0f;
    private float currentShakeRoughness = 0.5f;

    private Vector3 currentMouseOffset = Vector3.zero;
    private Vector3 targetMouseOffset = Vector3.zero;

    private Vector3 currentLookAheadOffset = Vector3.zero;
    private Vector3 previousTargetPosition = Vector3.zero;

    private float randomSeedX, randomSeedY;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerController.OnPlayerDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerController.OnPlayerDeath -= HandlePlayerDeath;
    }

    private void Start()
    {
        initialRotation = transform.rotation;
        randomSeedX = Random.Range(0f, 100f);
        randomSeedY = Random.Range(0f, 100f);
        FindAndAssignPlayer();

        if (target != null)
            previousTargetPosition = target.position;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndAssignPlayer();
        ResetCameraState();
    }

    private void HandlePlayerDeath()
    {
        ResetCameraState();
    }

    private void FindAndAssignPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(defaultTargetTag);
        if (playerObj == null)
        {
            return;
        }
        target = playerObj.transform;
        playerThermal = playerObj.GetComponent<GeneralThermalRegulator>();
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        if (shakeRemainingTime > 0f)
        {
            shakeRemainingTime -= Time.deltaTime;
            currentShakeStrength = Mathf.MoveTowards(currentShakeStrength, 0f, shakeDecaySpeed * Time.deltaTime);
            if (shakeRemainingTime <= 0f) currentShakeStrength = 0f;
        }
        else currentShakeStrength = 0f;

        Vector3 desiredPosition = target.position + offset;

        if (enableLookAhead)
        {
            Vector3 playerDelta = target.position - previousTargetPosition;
            previousTargetPosition = target.position;
            float horizontalSpeed = Mathf.Abs(playerDelta.x) / Time.deltaTime;
            bool isMoving = horizontalSpeed > 0.1f;

            float facingDir = Mathf.Sign(target.localScale.x);

            Vector3 targetLookAhead = Vector3.zero;
            if (isMoving)
            {
                targetLookAhead = new Vector3(facingDir * lookAheadDistance, 0f, 0f);
            }
            float lerpSpeed = isMoving ? lookAheadSpeed : lookAheadReturnSpeed;
            currentLookAheadOffset = Vector3.Lerp(currentLookAheadOffset, targetLookAhead, lerpSpeed * Time.deltaTime);

            desiredPosition += currentLookAheadOffset;
        }
        
        if (enableMouseFollow && Input.GetMouseButton(1))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            
            Vector2 playerPos = target.position;
            Vector2 mouseDir = (mouseWorldPos - (Vector3)playerPos).normalized;
            float mouseDistance = Vector2.Distance(mouseWorldPos, playerPos);
            
            if (mouseDistance > minFollowDistance)
            {
                float offsetMagnitude = Mathf.Clamp(mouseDistance - minFollowDistance, 0f, maxFollowDistance);
                targetMouseOffset = (Vector3)mouseDir * offsetMagnitude;
            }
            else
            {
                targetMouseOffset = Vector3.zero;
            }
        }
        else
        {
            targetMouseOffset = Vector3.zero;
        }
        
        currentMouseOffset = Vector3.Lerp(currentMouseOffset, targetMouseOffset, mouseFollowSmoothing * Time.deltaTime);
        desiredPosition += currentMouseOffset;
        
        float targetIntensity = 0f;
        if (playerThermal != null)
        {
            float temp = playerThermal.GetBaseTemperature();
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
        }
        
        if (currentShakeStrength > 0.001f)
        {
            Vector2 shakeOffset = GetShakeOffset(currentShakeStrength, currentShakeRoughness);
            desiredPosition += new Vector3(shakeOffset.x, shakeOffset.y, 0f);
        }
        
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        Quaternion targetRotation = initialRotation;
        if (currentTiltIntensity > 0.001f && enableTilt)
        {
            float tiltTime = Time.time * tiltFrequency;
            float tiltZ = Mathf.Sin(tiltTime) * currentTiltIntensity;
            tiltZ += Mathf.Sin(tiltTime * 2.3f) * (currentTiltIntensity * 0.5f);
            targetRotation = initialRotation * Quaternion.Euler(0f, 0f, tiltZ);
        }
        
        transform.position = smoothedPos;
        transform.rotation = targetRotation;
    }

    private Vector2 GetShakeOffset(float strength, float roughness)
    {
        float time = Time.time;
        float rough = Mathf.Clamp01(roughness);
        
        float smoothX = Mathf.Sin(time * 25f) * strength;
        smoothX += Mathf.Sin(time * 13f) * (strength * 0.6f);
        float smoothY = Mathf.Cos(time * 22f) * strength;
        smoothY += Mathf.Sin(time * 17f) * (strength * 0.5f);
        
        float noiseX = Mathf.PerlinNoise(randomSeedX + time * 30f, 0f) * 2f - 1f;
        float noiseY = Mathf.PerlinNoise(randomSeedY + time * 25f, 0f) * 2f - 1f;
        noiseX *= strength * rough;
        noiseY *= strength * rough;
        
        float finalX = (smoothX * (1f - rough)) + noiseX;
        float finalY = (smoothY * (1f - rough)) + noiseY;
        return new Vector2(finalX, finalY);
    }
    
    public void TriggerShake(float intensity, float duration, float smoothness = 0.5f)
    {
        currentShakeStrength = Mathf.Max(currentShakeStrength, intensity);
        shakeRemainingTime = Mathf.Max(shakeRemainingTime, duration);
        currentShakeRoughness = Mathf.Clamp01(smoothness);
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (newTarget != null)
            playerThermal = newTarget.GetComponentInChildren<GeneralThermalRegulator>();
    }

    public void ResetCameraState()
    {
        currentWobbleIntensity = 0f;
        currentTiltIntensity = 0f;
        currentShakeStrength = 0f;
        shakeRemainingTime = 0f;
        currentShakeRoughness = 0.5f;
        currentMouseOffset = Vector3.zero;
        targetMouseOffset = Vector3.zero;
        currentLookAheadOffset = Vector3.zero;

        if (target != null)
            previousTargetPosition = target.position;

        transform.rotation = initialRotation;
        if (target != null)
            transform.position = target.position + offset;
    }

    public void SnapToPlayer()
    {
        if (target == null)
            return;

        previousTargetPosition = target.position;
        currentLookAheadOffset = Vector3.zero;
        currentMouseOffset = Vector3.zero;
        targetMouseOffset = Vector3.zero;
        transform.position = target.position + offset;
        transform.rotation = initialRotation;
    }
    
    public void ResetToPlayer()
    {
        FindAndAssignPlayer();
    }
}