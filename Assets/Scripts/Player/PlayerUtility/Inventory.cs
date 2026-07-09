using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour, ISaveable
{
    public enum EquipmentType { None, Gun, Spray }

    [Header("Arm References")]
    [SerializeField] private GameObject emptyHandArm;
    [SerializeField] private GameObject gunArm;
    [SerializeField] private GameObject sprayArm;

    private EquipmentType currentEquipment = EquipmentType.None;
    private HashSet<string> ownedItems = new HashSet<string>();
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(EquipmentType.None);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (ownedItems.Contains("Gun")) Equip(EquipmentType.Gun);
            else Debug.Log("Gun not owned.");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (ownedItems.Contains("Spray")) Equip(EquipmentType.Spray);
            else Debug.Log("Spray not owned.");
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
            Equip(EquipmentType.Gun);
            SaveManager.Instance?.RequestSave();
        }
    }

    public void GiveSpray()
    {
        if (!ownedItems.Contains("Spray"))
        {
            ownedItems.Add("Spray");
            Equip(EquipmentType.Spray);
            SaveManager.Instance?.RequestSave();
        }
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    public EquipmentType GetCurrentEquipment()
    {
        return currentEquipment;
    }

    public void EquipType(EquipmentType type)
    {
        Equip(type);
    }

    public bool IsItemOwned(string itemName)
    {
        return ownedItems.Contains(itemName);
    }

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
}