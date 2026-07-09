using UnityEngine;
using System.Linq;

public class ItemGiver : MonoBehaviour, ISaveable, IInteractible
{
    [SerializeField] private string saveID;
    [SerializeField] private ItemData itemData;
    [SerializeField] private bool hideAfterPickup = true;

    private bool wasGiven = false;
    private bool interactible = false;
    private ButtonTrigger buttonTrigger; // <-- add this

    private void Start()
    {
        buttonTrigger = GetComponent<ButtonTrigger>(); // <-- cache reference

        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Register(this);

        if (wasGiven && hideAfterPickup)
            gameObject.SetActive(false);
        else if (!wasGiven)
            DisableInteraction(); // start with interaction disabled
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Unregister(this);
    }

    public void GiveAccessItem()
    {
        if (!interactible) return;
        if (wasGiven) return;

        if (itemData == null)
        {
            Debug.LogWarning($"ItemGiver: {name} has no ItemData assigned.");
            return;
        }

        if (string.IsNullOrEmpty(itemData.itemID))
        {
            Debug.LogWarning($"ItemGiver: {name} has empty itemID in ItemData.");
            return;
        }

        GameData data = SaveManager.Instance?.GetCurrentData();
        if (data == null) return;

        if (!data.collectedAccessItems.Contains(itemData.itemID))
        {
            data.collectedAccessItems.Add(itemData.itemID);
            Debug.Log($"ItemGiver: '{itemData.itemID}' added to collectedAccessItems.");

            if (NotificationManager.Instance != null)
                NotificationManager.Instance.ShowItemNotification(itemData);
        }
        else
        {
            Debug.Log($"ItemGiver: '{itemData.itemID}' already collected.");
        }

        wasGiven = true;
        if (hideAfterPickup)
            gameObject.SetActive(false);
    }

    // --- IInteractible ---
    public void EnableInteraction()
    {
        interactible = true;
        if (buttonTrigger != null)
            buttonTrigger.enabled = true;
    }

    public void DisableInteraction()
    {
        interactible = false;
        if (buttonTrigger != null)
            buttonTrigger.enabled = false;
    }

    // --- ISaveable ---
    public void Save(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = wasGiven ? "Given" : "NotGiven" });
    }

    public void Load(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            wasGiven = cs.state == "Given";
            if (wasGiven && hideAfterPickup) {
                gameObject.SetActive(false);
            }
            else
                DisableInteraction();
        }
    }

    public bool WasGiven => wasGiven;
}