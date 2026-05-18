using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour, ISaveable
{
    public enum EquipmentType { None, Gun }

    [Header("Arm References")]
    [SerializeField] private GameObject emptyHandArm;
    [SerializeField] private GameObject gunArm;

    private EquipmentType currentEquipment = EquipmentType.None;
    private HashSet<string> ownedItems = new HashSet<string>();

    private void Start()
    {
        SaveManager.Instance?.Register(this);
        // Inventory does NOT load itself anymore.
        // The SaveManager will call Load() after this object is registered.
        // But we need default values if no save exists.
        if (!ownedItems.Contains("None"))
            ownedItems.Add("None");
        //Equip(EquipmentType.None);
    }

    // Called by SaveManager when loading
    public void Load(GameData data)
    {
        ownedItems.Clear();
        if (data.ownedItems != null)
            ownedItems.UnionWith(data.ownedItems);
        
        EquipmentType loadedEquip = EquipmentType.None;
        if (data.currentEquipment == "Gun") loadedEquip = EquipmentType.Gun;
        
        if (!ownedItems.Contains("None"))
            ownedItems.Add("None");
        
        //Debug.Log($"Inventory loaded. Owned items: {string.Join(", ", ownedItems)}. Current equip: {loadedEquip}");
        Equip(loadedEquip);
    }

    // Called by SaveManager when saving
    public void Save(GameData data)
    {
        data.ownedItems = new List<string>(ownedItems);
        data.currentEquipment = currentEquipment.ToString();
    }

    // ----- Rest of Inventory logic (Equip, GiveGun, Update) unchanged -----
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(EquipmentType.None);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (ownedItems.Contains("Gun")) Equip(EquipmentType.Gun);
            else Debug.Log("Gun not owned. Press G to give yourself the gun.");
        }
        if (Input.GetKeyDown(KeyCode.G)) GiveGun();
    }

    private void Equip(EquipmentType type)
    {
        if (currentEquipment == type) return;
        if (emptyHandArm) emptyHandArm.SetActive(false);
        if (gunArm) gunArm.SetActive(false);
        switch (type)
        {
            case EquipmentType.None:
                if (emptyHandArm) emptyHandArm.SetActive(true);
                currentEquipment = EquipmentType.None;
                break;
            case EquipmentType.Gun:
                if (gunArm) gunArm.SetActive(true);
                currentEquipment = EquipmentType.Gun;
                break;
        }
        // No auto-save here – SaveManager will be called externally when needed.
    }

    private void GiveGun()
    {
        if (!ownedItems.Contains("Gun"))
        {
            ownedItems.Add("Gun");
            Equip(EquipmentType.Gun);
            // Notify SaveManager to save
            SaveManager.Instance?.RequestSave();
        }
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }
}