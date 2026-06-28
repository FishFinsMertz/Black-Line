using UnityEngine;

public class FreezeInteractible : MonoBehaviour, ITemperatureChangeable
{
    [Header("Initial Stats")]
    [SerializeField] private bool freezeEnabled = true;
    [SerializeField] private bool startFrozen = false;

    [Header("Temperature Bounds")]
    [SerializeField] private float lowTempBound = 0f;
    [SerializeField] private float highTempBound = 50f;

    [Header("References")]
    [SerializeField] private MonoBehaviour interactibleBehaviour;
    [SerializeField] private Animator animator;
    [SerializeField] private ThermalObject thermalObject; // assign or auto-find

    [Header("Animator Parameters")]
    [SerializeField] private string frozenBool = "Frozen";
    [SerializeField] private string unfreezeTrigger = "Unfreeze";

    private IInteractible interactible;
    private bool isFrozen;

    private void Start()
    {
        if (thermalObject == null)
            thermalObject = GetComponent<ThermalObject>();
        if (thermalObject == null)
            Debug.LogError("FreezeInteractible: No ThermalObject found!", this);

        if (interactibleBehaviour != null)
        {
            interactible = interactibleBehaviour as IInteractible;
            if (interactible == null)
                Debug.LogError($"FreezeInteractible: {interactibleBehaviour.name} does not implement IInteractible!", this);
        }
        else
        {
            Debug.LogError("FreezeInteractible: No interactibleBehaviour assigned!", this);
            enabled = false;
            return;
        }

        if (startFrozen)
        {
            isFrozen = true;
            interactible.DisableInteraction();
            if (animator != null)
                animator.SetBool(frozenBool, true);
            if (thermalObject != null)
                thermalObject.SetBaseTemperature(lowTempBound);
        }
        else
        {
            isFrozen = false;
            interactible.EnableInteraction();
            if (animator != null)
                animator.SetBool(frozenBool, false);
            if (thermalObject != null)
                thermalObject.SetBaseTemperature(highTempBound);
        }
    }

    public void ChangeBaseTemperature(float amount)
    {
        if (!freezeEnabled || thermalObject == null) return;

        thermalObject.ChangeBaseTemperature(amount);

        float currentTemp = thermalObject.GetTemperature();

        if (isFrozen && currentTemp >= highTempBound)
        {
            Unfreeze();
        }
        else if (!isFrozen && currentTemp <= lowTempBound)
        {
            Freeze();
        }
    }

    private void Freeze()
    {
        if (isFrozen) return;
        isFrozen = true;
        interactible.DisableInteraction();

        if (animator != null)
            animator.SetBool(frozenBool, true);
    }

    private void Unfreeze()
    {
        if (!isFrozen) return;
        isFrozen = false;
        interactible.EnableInteraction();

        if (animator != null)
        {
            animator.SetBool(frozenBool, false);
            animator.SetTrigger(unfreezeTrigger);
        }
    }
}