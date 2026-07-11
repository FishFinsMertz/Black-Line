using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField] private CanvasGroup parentGroup;
    [SerializeField] private float fadeDelay = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Slots")]
    [SerializeField] private CanvasGroup spraySlot;
    [SerializeField] private CanvasGroup gunSlot;
    [SerializeField] private float dimAlpha = 0.3f;

    [Header("Slot Icons")]
    [SerializeField] private Image sprayIcon;
    [SerializeField] private Image gunIcon;

    [Header("Ammo & Name")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private string unequippedName;
    [SerializeField] private string unequippedAmmo;
    [SerializeField] private string sprayDisplayName;
    [SerializeField] private string gunDisplayName;

    private Inventory inventory;
    private Inventory.EquipmentType lastEquipment;
    private float inactivityTimer = 0f;
    private bool isFadingOut = false;

    private bool lastSprayOwned = false;
    private bool lastGunOwned = false;

    private void Start()
    {
        inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("WeaponUI: No Inventory found.");
            return;
        }

        lastEquipment = inventory.GetCurrentEquipment();

        // Start hidden
        parentGroup.alpha = 0f;
        inactivityTimer = 0f;

        ApplyOwnershipIcons();
        UpdateSlots();
        UpdateAmmoDisplay();
    }

    private void Update()
    {
        if (inventory == null) return;

        var current = inventory.GetCurrentEquipment();
        if (current != lastEquipment)
        {
            lastEquipment = current;
            UpdateSlots();
            UpdateAmmoDisplay();
            ShowUI();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            ShowUI();
        }

        UpdateAmmoDisplay();
        CheckOwnershipChange();

        if (inactivityTimer > 0)
        {
            inactivityTimer -= Time.deltaTime;
            if (inactivityTimer <= 0 && !isFadingOut)
            {
                isFadingOut = true;
            }
        }

        if (isFadingOut)
        {
            parentGroup.alpha = Mathf.Lerp(parentGroup.alpha, 0f, Time.deltaTime / fadeDuration);
            if (parentGroup.alpha <= 0.01f)
            {
                parentGroup.alpha = 0f;
                isFadingOut = false;
            }
        }
    }

    private void ShowUI()
    {
        inactivityTimer = fadeDelay;
        isFadingOut = false;
        parentGroup.alpha = 1f;
    }

    private void UpdateSlots()
    {
        var current = inventory.GetCurrentEquipment();
        SetSlotAlpha(spraySlot, current == Inventory.EquipmentType.Spray);
        SetSlotAlpha(gunSlot, current == Inventory.EquipmentType.Gun);
    }

    private void SetSlotAlpha(CanvasGroup slot, bool isEquipped)
    {
        if (slot == null) return;
        slot.alpha = isEquipped ? 1f : dimAlpha;
    }

    private void UpdateAmmoDisplay()
    {
        var current = inventory.GetCurrentEquipment();

        if (current == Inventory.EquipmentType.None)
        {
            if (weaponNameText != null) weaponNameText.text = unequippedName;
            if (ammoText != null) ammoText.text = unequippedAmmo;
            return;
        }

        string weaponID = current.ToString();
        string displayName = weaponID switch
        {
            "Spray" => sprayDisplayName,
            "Gun" => gunDisplayName,
            _ => weaponID
        };

        var ammo = inventory.GetAmmo(weaponID);
        if (weaponNameText != null) weaponNameText.text = displayName;
        if (ammoText != null) ammoText.text = $"{ammo.magazine} / {ammo.reserve}";
    }

    private void ApplyOwnershipIcons()
    {
        if (inventory == null) return;

        bool sprayOwned = inventory.IsItemOwned("Spray");
        bool gunOwned = inventory.IsItemOwned("Gun");

        if (sprayIcon != null) sprayIcon.enabled = sprayOwned;
        if (gunIcon != null) gunIcon.enabled = gunOwned;

        lastSprayOwned = sprayOwned;
        lastGunOwned = gunOwned;
    }

    private void CheckOwnershipChange()
    {
        if (inventory == null) return;

        bool sprayOwned = inventory.IsItemOwned("Spray");
        bool gunOwned = inventory.IsItemOwned("Gun");

        if (sprayOwned != lastSprayOwned || gunOwned != lastGunOwned)
        {
            lastSprayOwned = sprayOwned;
            lastGunOwned = gunOwned;

            if (sprayIcon != null) sprayIcon.enabled = sprayOwned;
            if (gunIcon != null) gunIcon.enabled = gunOwned;
        }
    }
}