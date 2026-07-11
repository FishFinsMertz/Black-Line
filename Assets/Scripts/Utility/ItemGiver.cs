using UnityEngine;
using System.Linq;

public class ItemGiver : MonoBehaviour, ISaveable, IInteractible
{
    public enum ItemType { AccessItem, Weapon, Ammo }

    [SerializeField] private string saveID;
    [SerializeField] private ItemData itemData;
    [SerializeField] private ItemType itemType = ItemType.AccessItem;
    [SerializeField] private bool hideAfterPickup = true;
    [SerializeField] private bool startInteractible = true;

    [Header("Ammo Settings")]
    [SerializeField] private string weaponID = "Gun";
    [SerializeField] private int ammoAmount = 10;

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

    public void GiveAmmo()
    {
        if (!interactible) return;
        if (wasGiven) return;

        if (playerInventory == null)
        {
            Debug.LogError($"ItemGiver: {name} no Inventory found.");
            return;
        }

        if (string.IsNullOrEmpty(weaponID))
        {
            Debug.LogWarning($"ItemGiver: {name} no weaponID set for ammo pickup.");
            return;
        }

        if (ammoAmount <= 0)
        {
            Debug.LogWarning($"ItemGiver: {name} ammoAmount is 0 or negative.");
            return;
        }

        playerInventory.AddAmmo(weaponID, 0, ammoAmount);

        if (NotificationManager.Instance != null)
        {
            string displayName = weaponID;
            if (weaponID == "Gun") displayName = "Pistol";
            else if (weaponID == "Spray") displayName = "Spray";
            NotificationManager.Instance.NotifyBottom($"+{ammoAmount} {displayName} Ammo");
        }

        wasGiven = true;
        if (hideAfterPickup) gameObject.SetActive(false);
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