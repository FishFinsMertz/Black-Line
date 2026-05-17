using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GameObject emptyHandArm;   // Arm_EmptyHand
    [SerializeField] private GameObject gunArm;         // Arm_Gun (with ArmGunController)
    public enum EquipmentType { None, Gun }
    private EquipmentType currentEquipment = EquipmentType.None;

    // Owned items (simple set of strings, expandable for save system)
    private System.Collections.Generic.HashSet<string> ownedItems = new System.Collections.Generic.HashSet<string>();

    // Start is called before the first frame update
    void Start()
    {
        // By default, the empty hand is always owned
        ownedItems.Add("None");

        // Start with empty hand equipped
        Equip(EquipmentType.None);
    }

    // Update is called once per frame
    void Update()
    {
        // Equip via number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Equip(EquipmentType.None);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (ownedItems.Contains("Gun"))
                Equip(EquipmentType.Gun);
        }

        // Test: give gun on G key
        if (Input.GetKeyDown(KeyCode.G))
        {
            GiveGun();
        }
    }

    // Equip a specific equipment type
    private void Equip(EquipmentType type)
    {
        // If already equipped, do nothing
        if (currentEquipment == type) return;

        // Disable all arms first
        if (emptyHandArm != null) emptyHandArm.SetActive(false);
        if (gunArm != null) gunArm.SetActive(false);

        // Enable the corresponding arm and update currentEquipment
        switch (type)
        {
            case EquipmentType.None:
                if (emptyHandArm != null) emptyHandArm.SetActive(true);
                currentEquipment = EquipmentType.None;
                //Debug.Log("Equipped: Empty hands");
                break;
            case EquipmentType.Gun:
                if (gunArm != null) gunArm.SetActive(true);
                currentEquipment = EquipmentType.Gun;
                //Debug.Log("Equipped: Gun");
                break;
        }
    }

    // Give the gun item (for testing)
    private void GiveGun()
    {
        if (!ownedItems.Contains("Gun"))
        {
            ownedItems.Add("Gun");
            Debug.Log("Gun added to inventory.");
            // Optionally auto‑equip the gun
            Equip(EquipmentType.Gun);
        }
        else
        {
            Debug.Log("You already have the gun.");
        }
    }

    // Public API
    public bool HasItem(string itemName) => ownedItems.Contains(itemName);
    public void AddItem(string itemName) => ownedItems.Add(itemName);
    public void RemoveItem(string itemName) => ownedItems.Remove(itemName);
    public EquipmentType GetCurrentEquipment() => currentEquipment;

    // For saving: get owned items as a list
    public System.Collections.Generic.List<string> GetOwnedItemsList()
    {
        return new System.Collections.Generic.List<string>(ownedItems);
    }

    // For loading: set owned items from saved list
    public void SetOwnedItems(System.Collections.Generic.List<string> items)
    {
        ownedItems.Clear();
        foreach (string item in items)
            ownedItems.Add(item);
        // You may want to re‑equip the last saved equipment here
    }
}