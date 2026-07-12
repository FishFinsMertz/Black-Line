using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField] private CanvasGroup parentGroup;
    [SerializeField] private float fadeDelay = 3f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float hideFadeDuration = 0.2f;

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
    [SerializeField] private string unequippedName = "None";
    [SerializeField] private string unequippedAmmo = "∞";
    [SerializeField] private string sprayDisplayName = "Spray";
    [SerializeField] private string gunDisplayName = "Gun";

    private Inventory inventory;
    private PlayerController player;
    private Inventory.EquipmentType lastEquipment;
    private float inactivityTimer = 0f;
    private float targetAlpha = 0f;
    private bool wasClimbing = false;
    private bool suppressNextEquipmentChange = false;

    private bool lastSprayOwned = false;
    private bool lastGunOwned = false;

    private void Start()
    {
        inventory = FindFirstObjectByType<Inventory>();
        player = FindFirstObjectByType<PlayerController>();

        if (inventory == null)
        {
            Debug.LogWarning("WeaponUI: No Inventory found.");
            return;
        }

        lastEquipment = inventory.GetCurrentEquipment();
        parentGroup.alpha = 0f;
        inactivityTimer = 0f;
        targetAlpha = 0f;

        ApplyOwnershipIcons();
        UpdateSlots();
        UpdateAmmoDisplay();
    }

    private void Update()
    {
        if (inventory == null) return;

        bool isClimbing = player != null && player.GetCurrentState() is PlayerClimbingState;
        var current = inventory.GetCurrentEquipment();

        if (isClimbing)
        {
            targetAlpha = 0f;
            wasClimbing = true;
            suppressNextEquipmentChange = true;
        }
        else
        {
            if (wasClimbing)
            {
                inactivityTimer = 0f;
                targetAlpha = 0f;
                wasClimbing = false;
                suppressNextEquipmentChange = true;
            }

            if (current != lastEquipment)
            {
                lastEquipment = current;
                UpdateSlots();
                UpdateAmmoDisplay();

                if (suppressNextEquipmentChange)
                {
                    suppressNextEquipmentChange = false;
                }
                else
                {
                    inactivityTimer = fadeDelay;
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                inactivityTimer = fadeDelay;
            }

            UpdateAmmoDisplay();
            CheckOwnershipChange();

            if (current == Inventory.EquipmentType.None)
            {
                targetAlpha = 0f;
            }
            else if (inactivityTimer > 0)
            {
                inactivityTimer -= Time.deltaTime;
                targetAlpha = 1f;
            }
            else
            {
                targetAlpha = 0f;
            }
        }

        float duration = targetAlpha > parentGroup.alpha ? fadeDuration : hideFadeDuration;
        parentGroup.alpha = Mathf.Lerp(parentGroup.alpha, targetAlpha, Time.deltaTime / duration);
        if (parentGroup.alpha < 0.001f && targetAlpha == 0f)
            parentGroup.alpha = 0f;
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
            if (weaponNameText != null) weaponNameText.text = "";
            if (ammoText != null) ammoText.text = "";
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