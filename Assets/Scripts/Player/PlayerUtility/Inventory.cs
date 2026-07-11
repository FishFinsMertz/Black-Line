using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour, ISaveable
{
    public enum EquipmentType { None, Gun, Spray }

    [Header("Arm References")]
    [SerializeField] private GameObject emptyHandArm;
    [SerializeField] private GameObject gunArm;
    [SerializeField] private GameObject sprayArm;

    private EquipmentType currentEquipment = EquipmentType.None;
    private HashSet<string> ownedItems = new HashSet<string>();
    private Dictionary<string, (int magazine, int reserve)> ammoData = new Dictionary<string, (int, int)>();
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        SaveManager.Instance?.Register(this);
        if (!ownedItems.Contains("None"))
            ownedItems.Add("None");
    }

    public void Load(GameData data)
    {
        ownedItems.Clear();
        if (data.ownedItems != null)
            ownedItems.UnionWith(data.ownedItems);

        ammoData.Clear();
        if (data.weaponAmmo != null)
        {
            foreach (var entry in data.weaponAmmo)
                ammoData[entry.weaponID] = (entry.magazine, entry.reserve);
        }

        EquipmentType loadedEquip = EquipmentType.None;
        if (data.currentEquipment == "Gun") loadedEquip = EquipmentType.Gun;
        else if (data.currentEquipment == "Spray") loadedEquip = EquipmentType.Spray;

        if (!ownedItems.Contains("None"))
            ownedItems.Add("None");

        Equip(loadedEquip);
    }

    public void Save(GameData data)
    {
        data.ownedItems = new List<string>(ownedItems);
        data.currentEquipment = currentEquipment.ToString();

        data.weaponAmmo = new List<AmmoEntry>();
        foreach (var kvp in ammoData)
        {
            data.weaponAmmo.Add(new AmmoEntry
            {
                weaponID = kvp.Key,
                magazine = kvp.Value.magazine,
                reserve = kvp.Value.reserve
            });
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(EquipmentType.None);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (ownedItems.Contains("Spray")) Equip(EquipmentType.Spray);
            else Debug.Log("Spray not owned.");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (ownedItems.Contains("Gun")) Equip(EquipmentType.Gun);
            else Debug.Log("Gun not owned.");
        }

        if (Input.GetKeyDown(KeyCode.G)) GiveGun();
        if (Input.GetKeyDown(KeyCode.Y)) GiveSpray();
    }

    private void Equip(EquipmentType type)
    {
        if (currentEquipment == type) return;
        if (emptyHandArm) emptyHandArm.SetActive(false);
        if (gunArm) gunArm.SetActive(false);
        if (sprayArm) sprayArm.SetActive(false);

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
            case EquipmentType.Spray:
                if (sprayArm) sprayArm.SetActive(true);
                currentEquipment = EquipmentType.Spray;
                break;
        }

        playerController.bodyAnimator.Play(0);
    }

    public void GiveGun()
    {
        if (!ownedItems.Contains("Gun"))
        {
            ownedItems.Add("Gun");
            ammoData["Gun"] = (12, 24);
            Equip(EquipmentType.Gun);
            SaveManager.Instance?.RequestSave();
        }
    }

    public void GiveSpray()
    {
        if (!ownedItems.Contains("Spray"))
        {
            ownedItems.Add("Spray");
            ammoData["Spray"] = (30, 60);
            Equip(EquipmentType.Spray);
            SaveManager.Instance?.RequestSave();
        }
    }

    // ---- Ammo API ----
    public (int magazine, int reserve) GetAmmo(string weaponID)
    {
        if (ammoData.TryGetValue(weaponID, out var ammo))
            return ammo;
        return (0, 0);
    }

    public bool UseAmmo(string weaponID, int amount = 1)
    {
        if (!ammoData.ContainsKey(weaponID)) return false;
        var ammo = ammoData[weaponID];
        if (ammo.magazine < amount) return false;
        ammo.magazine -= amount;
        ammoData[weaponID] = ammo;
        return true;
    }

    public void Reload(string weaponID)
    {
        if (!ammoData.ContainsKey(weaponID)) return;
        var ammo = ammoData[weaponID];
        if (ammo.reserve <= 0) return;
        int capacity = GetWeaponCapacity(weaponID);
        int needed = capacity - ammo.magazine;
        if (needed <= 0) return;
        int transfer = Mathf.Min(needed, ammo.reserve);
        ammo.magazine += transfer;
        ammo.reserve -= transfer;
        ammoData[weaponID] = ammo;
    }

    public void AddAmmo(string weaponID, int magAdd, int reserveAdd)
    {
        if (!ammoData.ContainsKey(weaponID)) return;
        var ammo = ammoData[weaponID];
        ammo.magazine = Mathf.Min(GetWeaponCapacity(weaponID), ammo.magazine + magAdd);
        ammo.reserve += reserveAdd;
        ammoData[weaponID] = ammo;
    }

    private int GetWeaponCapacity(string weaponID)
    {
        if (weaponID == "Gun") return 12;
        if (weaponID == "Spray") return 30;
        return 0;
    }

    public bool IsItemOwned(string itemName) => ownedItems.Contains(itemName);
    public void AddItem(string itemName)
    {
        if (!ownedItems.Contains(itemName))
        {
            ownedItems.Add(itemName);
            SaveManager.Instance?.RequestSave();
        }
    }

    public void EquipByName(string itemName)
    {
        if (itemName == "Gun") Equip(EquipmentType.Gun);
        else if (itemName == "Spray") Equip(EquipmentType.Spray);
    }

    public EquipmentType GetCurrentEquipment() => currentEquipment;
    public void EquipType(EquipmentType type) => Equip(type);

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }
}