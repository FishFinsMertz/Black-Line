using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour, ISaveable
{
    public enum EquipmentType { None, Spray, Gun }

    [Header("Arm References")]
    [SerializeField] private GameObject emptyHandArm;
    [SerializeField] private GameObject sprayArm;
    [SerializeField] private GameObject gunArm;

    [Header("Starting Ammo")]
    [SerializeField] private int gunStartingMagazine = 12;
    [SerializeField] private int gunStartingReserve = 24;
    [SerializeField] private int sprayStartingMagazine = 30;
    [SerializeField] private int sprayStartingReserve = 60;

    [Header("Recharger Settings")]
    [SerializeField] private string rechargerID = "BatteryRecharger";
    [SerializeField] private int maxRechargers = 5;
    [SerializeField] private float rechargeAmount = 20f;
    [SerializeField] private KeyCode useRechargerKey = KeyCode.F;

    private EquipmentType currentEquipment = EquipmentType.None;
    private HashSet<string> ownedItems = new HashSet<string>();
    private Dictionary<string, (int magazine, int reserve)> ammoData = new Dictionary<string, (int, int)>();
    private Dictionary<string, int> consumables = new Dictionary<string, int>();
    private PlayerController playerController;
    private GeneralThermalRegulator thermalRegulator;
    private List<EquipmentType> ownedWeapons = new List<EquipmentType>();

    public System.Action OnConsumablesChanged;
    public System.Action<string, int> OnConsumableUsed;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        thermalRegulator = GetComponent<GeneralThermalRegulator>();
        SaveManager.Instance?.Register(this);
        if (!ownedItems.Contains("None"))
            ownedItems.Add("None");
        UpdateOwnedWeaponsList();
    }

    private void UpdateOwnedWeaponsList()
    {
        ownedWeapons.Clear();
        ownedWeapons.Add(EquipmentType.None);
        if (ownedItems.Contains("Spray")) ownedWeapons.Add(EquipmentType.Spray);
        if (ownedItems.Contains("Gun")) ownedWeapons.Add(EquipmentType.Gun);
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

        consumables.Clear();
        if (data.consumables != null)
        {
            foreach (var entry in data.consumables)
                consumables[entry.id] = entry.amount;
        }

        EquipmentType loadedEquip = EquipmentType.None;
        if (data.currentEquipment == "Spray") loadedEquip = EquipmentType.Spray;
        else if (data.currentEquipment == "Gun") loadedEquip = EquipmentType.Gun;

        if (!ownedItems.Contains("None"))
            ownedItems.Add("None");

        UpdateOwnedWeaponsList();
        Equip(loadedEquip);
        OnConsumablesChanged?.Invoke();
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

        data.consumables = new List<ConsumableEntry>();
        foreach (var kvp in consumables)
        {
            data.consumables.Add(new ConsumableEntry
            {
                id = kvp.Key,
                amount = kvp.Value
            });
        }
    }

    // ---- Consumable API ----
    public int GetConsumableCount(string id)
    {
        if (string.IsNullOrEmpty(id))
            return 0;

        return consumables.TryGetValue(id, out int count) ? count : 0;
    }

    public string RechargerID => rechargerID;
    public float RechargeAmount => rechargeAmount;
    public int MaxRechargers => maxRechargers;

    public bool TryAddConsumable(string id, int amount)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        if (id == rechargerID)
        {
            int currentCount = consumables.TryGetValue(id, out int count) ? count : 0;
            int availableSlots = maxRechargers - currentCount;
            if (availableSlots <= 0)
            {
                NotificationManager.Instance?.NotifyBottom($"You already have the maximum {maxRechargers} rechargers.");
                OnConsumablesChanged?.Invoke();
                return false;
            }

            amount = Mathf.Min(amount, availableSlots);
            if (amount <= 0)
            {
                NotificationManager.Instance?.NotifyBottom($"You already have the maximum {maxRechargers} rechargers.");
                OnConsumablesChanged?.Invoke();
                return false;
            }
        }

        if (consumables.ContainsKey(id))
            consumables[id] += amount;
        else
            consumables[id] = amount;
        OnConsumablesChanged?.Invoke();
        return true;
    }

    public bool TryUseRecharger()
    {
        if (thermalRegulator == null) return false;

        if (GetConsumableCount(rechargerID) <= 0)
        {
            NotificationManager.Instance?.NotifyBottom("No battery rechargers left.");
            return false;
        }

        if (thermalRegulator.GetBatteryLevel() >= 100f)
        {
            NotificationManager.Instance?.NotifyBottom("Battery already full!");
            return false;
        }

        if (UseConsumable(rechargerID, 1))
        {
            thermalRegulator.AddBattery(rechargeAmount);
            NotificationManager.Instance?.NotifyBottom($"Battery +{rechargeAmount}%");
            SaveManager.Instance?.RequestSave();
            return true;
        }

        return false;
    }

    public bool UseConsumable(string id, int amount = 1)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        if (!consumables.ContainsKey(id) || consumables[id] < amount)
            return false;
        consumables[id] -= amount;
        if (consumables[id] <= 0)
            consumables.Remove(id);
        OnConsumablesChanged?.Invoke();
        OnConsumableUsed?.Invoke(id, amount);
        return true;
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
        if (!ammoData.ContainsKey(weaponID))
            ammoData[weaponID] = (0, 0);
        var ammo = ammoData[weaponID];
        int capacity = GetWeaponCapacity(weaponID);
        ammo.magazine = Mathf.Min(capacity, ammo.magazine + magAdd);
        ammo.reserve += reserveAdd;
        ammoData[weaponID] = ammo;
    }

    private int GetWeaponCapacity(string weaponID)
    {
        if (weaponID == "Gun") return gunStartingMagazine;
        if (weaponID == "Spray") return sprayStartingMagazine;
        return 0;
    }

    // ---- Equipment & Inventory ----
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (ownedItems.Contains("Spray"))
            {
                if (currentEquipment == EquipmentType.Spray)
                    Equip(EquipmentType.None);
                else
                    Equip(EquipmentType.Spray);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (ownedItems.Contains("Gun"))
            {
                if (currentEquipment == EquipmentType.Gun)
                    Equip(EquipmentType.None);
                else
                    Equip(EquipmentType.Gun);
            }
        }

        if (Input.GetKeyDown(useRechargerKey))
            TryUseRecharger();

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (ownedWeapons.Count > 0)
            {
                int index = ownedWeapons.IndexOf(currentEquipment);
                if (index < 0) index = 0;
                if (scroll > 0)
                    index = (index + 1) % ownedWeapons.Count;
                else
                    index = (index - 1 + ownedWeapons.Count) % ownedWeapons.Count;
                Equip(ownedWeapons[index]);
            }
        }

        if (Input.GetKeyDown(KeyCode.G)) GiveGun();
        if (Input.GetKeyDown(KeyCode.Y)) GiveSpray();
    }

    private void Equip(EquipmentType type)
    {
        if (currentEquipment == type) return;
        if (emptyHandArm) emptyHandArm.SetActive(false);
        if (sprayArm) sprayArm.SetActive(false);
        if (gunArm) gunArm.SetActive(false);

        switch (type)
        {
            case EquipmentType.None:
                if (emptyHandArm) emptyHandArm.SetActive(true);
                break;
            case EquipmentType.Spray:
                if (sprayArm) sprayArm.SetActive(true);
                break;
            case EquipmentType.Gun:
                if (gunArm) gunArm.SetActive(true);
                break;
        }

        currentEquipment = type;
        playerController.bodyAnimator.Play(0);
    }

    public void GiveGun()
    {
        if (!ownedItems.Contains("Gun"))
        {
            ownedItems.Add("Gun");
            AddAmmo("Gun", gunStartingMagazine, gunStartingReserve);
            UpdateOwnedWeaponsList();
            Equip(EquipmentType.Gun);
            SaveManager.Instance?.RequestSave();
        }
    }

    public void GiveSpray()
    {
        if (!ownedItems.Contains("Spray"))
        {
            ownedItems.Add("Spray");
            AddAmmo("Spray", sprayStartingMagazine, sprayStartingReserve);
            UpdateOwnedWeaponsList();
            Equip(EquipmentType.Spray);
            SaveManager.Instance?.RequestSave();
        }
    }

    public bool IsItemOwned(string itemName) => ownedItems.Contains(itemName);
    public void AddItem(string itemName)
    {
        if (!ownedItems.Contains(itemName))
        {
            ownedItems.Add(itemName);
            UpdateOwnedWeaponsList();
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