using UnityEngine;
using System.Linq;

public class FreezeInteractible : MonoBehaviour, ITemperatureChangeable, ISaveable
{
    [Header("Save ID")]
    [SerializeField] private string saveID;

    [Header("Initial Stats")]
    [SerializeField] private bool freezeEnabled = true;
    [SerializeField] private bool startFrozen = false;

    [Header("Temperature Bounds")]
    [SerializeField] private float lowTempBound = 0f;
    [SerializeField] private float highTempBound = 50f;

    [Header("References")]
    [SerializeField] private MonoBehaviour interactibleBehaviour;
    [SerializeField] private Animator animator;
    [SerializeField] private ThermalObject thermalObject;

    [Header("Animator Parameters")]
    [SerializeField] private string frozenBool = "Frozen";
    [SerializeField] private string unfreezeTrigger = "Unfreeze";

    private IInteractible interactible;
    private bool isFrozen;

    private void Start()
    {
        // Validate references FIRST
        if (thermalObject == null)
            thermalObject = GetComponent<ThermalObject>();

        if (interactibleBehaviour != null)
            interactible = interactibleBehaviour as IInteractible;

        // Apply initial state (sets up interactible, animator, etc.)
        if (startFrozen)
        {
            isFrozen = true;
            ApplyFreezeState();
            thermalObject.SetBaseTemperature(lowTempBound);
        }
        else
        {
            isFrozen = false;
            ApplyUnfreezeState();
            thermalObject.SetBaseTemperature(highTempBound);
        }

        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Unregister(this);
    }

    public void ChangeBaseTemperature(float amount)
    {
        if (!freezeEnabled) return;

        thermalObject.ChangeBaseTemperature(amount);
        float currentTemp = thermalObject.GetTemperature();

        if (isFrozen && currentTemp >= highTempBound)
            Unfreeze();
        else if (!isFrozen && currentTemp <= lowTempBound)
            Freeze();
    }

    private void Freeze()
    {
        if (isFrozen) return;
        isFrozen = true;
        ApplyFreezeState();
    }

    private void Unfreeze()
    {
        if (!isFrozen) return;
        isFrozen = false;
        ApplyUnfreezeState();
    }

    private void ApplyFreezeState()
    {
        if (interactible != null)
            interactible.DisableInteraction();
        if (animator != null)
        {
            animator.SetBool(frozenBool, true);
            animator.ResetTrigger(unfreezeTrigger);
        }
    }

    private void ApplyUnfreezeState()
    {
        if (interactible != null)
            interactible.EnableInteraction();
        if (animator != null)
        {
            animator.SetBool(frozenBool, false);
            animator.SetTrigger(unfreezeTrigger);
        }
    }

    // --- ISaveable ---
    public void Save(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = isFrozen ? "Frozen" : "Unfrozen" });
    }

    public void Load(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            bool loadedFrozen = cs.state == "Frozen";
            if (loadedFrozen != isFrozen)
            {
                isFrozen = loadedFrozen;
                if (isFrozen)
                {
                    ApplyFreezeState();
                    if (thermalObject != null)
                        thermalObject.SetBaseTemperature(lowTempBound);
                }
                else
                {
                    ApplyUnfreezeState();
                    if (thermalObject != null)
                        thermalObject.SetBaseTemperature(highTempBound);
                }
            }
        }
    }
}