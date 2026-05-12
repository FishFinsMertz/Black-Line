using UnityEngine;

public class ThermalManager : MonoBehaviour
{
    public static ThermalManager Instance { get; private set; }

    // Event that fires whenever thermal vision is toggled (bool = enabled)
    public static System.Action<bool> OnThermalToggled;

    [SerializeField] private KeyCode toggleKey = KeyCode.T;
    [SerializeField] private bool startEnabled = false;

    private bool isThermalEnabled;

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

    private void Start()
    {
        SetThermalEnabled(startEnabled);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetThermalEnabled(!isThermalEnabled);
        }
    }

    public void SetThermalEnabled(bool enabled)
    {
        if (isThermalEnabled == enabled) return;
        isThermalEnabled = enabled;
        OnThermalToggled?.Invoke(isThermalEnabled);
        Debug.Log($"Thermal vision {(isThermalEnabled ? "ON" : "OFF")}");
    }

    public bool IsThermalEnabled() => isThermalEnabled;
}