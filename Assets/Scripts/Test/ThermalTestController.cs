using UnityEngine;

[RequireComponent(typeof(ThermalObject))]
public class ThermalTestController : MonoBehaviour
{
    [SerializeField] private KeyCode increaseKey = KeyCode.F;
    [SerializeField] private KeyCode decreaseKey = KeyCode.G;
    [SerializeField] private float stepAmount = 30f;

    private ThermalObject thermal;

    void Start() => thermal = GetComponent<ThermalObject>();

    void Update()
    {
        //if (Input.GetKeyDown(increaseKey))
            //thermal.SetTemperature(thermal.GetTemperature() + stepAmount);
        //else if (Input.GetKeyDown(decreaseKey))
            //thermal.SetTemperature(thermal.GetTemperature() - stepAmount);
    }
}