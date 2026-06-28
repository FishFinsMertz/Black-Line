using UnityEngine;

public class FreezeInteractible : MonoBehaviour
{
    [Header("Initial Stats")]
    [SerializeField] private bool freezeEnabled = false;
    [SerializeField] private bool startFrozen = false;

    [Header("Temperature Bounds")]
    [SerializeField] private float lowTempBound = 0;
    [SerializeField] private float highTempBound = 100;

    [Header("References")]
    [SerializeField] private IInteractible interactible;

    void Start()
    {
        if (startFrozen)
        {
            interactible.DisableInteraction();
        }
        //
    }

    // Public API
    public void ChangeBaseTemperature(float amount)
    {
        
    }
}
