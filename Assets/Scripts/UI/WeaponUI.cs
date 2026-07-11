using UnityEngine;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private string unequippedName = "None";
    [SerializeField] private string unequippedAmmo = "inf";

    private Inventory inventory;

    private void Start()
    {
        inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null)
            Debug.LogWarning("WeaponUI: No Inventory found in scene.");
    }

    private void Update()
    {
        if (inventory == null) return;

        var current = inventory.GetCurrentEquipment();
        string weaponID = current.ToString();

        if (weaponID == "None")
        {
            weaponNameText.text = unequippedName;
            ammoText.text = unequippedAmmo;
            return;
        }

        var ammo = inventory.GetAmmo(weaponID);
        weaponNameText.text = weaponID;
        ammoText.text = $"{ammo.magazine} / {ammo.reserve}";
    }
}