using UnityEngine;
using System.Linq;

public class ItemGiver : MonoBehaviour, ISaveable, IInteractible
{
    public enum ItemType { AccessItem, Weapon }

    [SerializeField] private string saveID;
    [SerializeField] private ItemData itemData;
    [SerializeField] private ItemType itemType = ItemType.AccessItem;
    [SerializeField] private bool hideAfterPickup = true;
    [SerializeField] private bool startInteractible = true; // NEW

    private bool wasGiven = false;
    private bool interactible = false;
    private ButtonTrigger buttonTrigger;
    private Inventory playerInventory;

    private void Start()
    {
        buttonTrigger = GetComponent<ButtonTrigger>();
        playerInventory = FindFirstObjectByType<Inventory>();

        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Register(this);

        if (wasGiven && hideAfterPickup)
            gameObject.SetActive(false);
        else
        {
            // Use startInteractible to decide initial state
            if (startInteractible)
                EnableInteraction();
            else
                DisableInteraction();
        }
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

        if (itemData == null || string.IsNullOrEmpty(itemData.itemID))
        {
            Debug.LogWarning($"ItemGiver: {name} invalid ItemData.");
            return;
        }

        GameData data = SaveManager.Instance?.GetCurrentData();
        if (data == null) return;

        if (!data.collectedAccessItems.Contains(itemData.itemID))
        {
            data.collectedAccessItems.Add(itemData.itemID);
            NotificationManager.Instance?.ShowItemNotification(itemData);
            wasGiven = true;
            if (hideAfterPickup) gameObject.SetActive(false);
        }
    }

    public void GiveWeapon()
    {
        if (!interactible) return;
        if (wasGiven) return;

        if (itemData == null || string.IsNullOrEmpty(itemData.itemID))
        {
            Debug.LogWarning($"ItemGiver: {name} invalid ItemData.");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogError($"ItemGiver: {name} no Inventory found.");
            return;
        }

        if (!playerInventory.IsItemOwned(itemData.itemID))
        {
            playerInventory.AddItem(itemData.itemID);
            playerInventory.EquipByName(itemData.itemID);
            NotificationManager.Instance?.ShowItemNotification(itemData);
            wasGiven = true;
            if (hideAfterPickup) gameObject.SetActive(false);
        }
    }

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
            if (wasGiven && hideAfterPickup)
                gameObject.SetActive(false);
            else
            {
                if (startInteractible)
                    EnableInteraction();
                else
                    DisableInteraction();
            }
        }
    }

    public bool WasGiven => wasGiven;
}