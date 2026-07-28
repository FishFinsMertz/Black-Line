using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private string sprayDisplayName = "Spray";
    [SerializeField] private string gunDisplayName = "Gun";

    private Inventory inventory;
    private PlayerController player;
    private float inactivityTimer = 0f;
    private float targetAlpha = 0f;
    private bool wasClimbing = false;

    private bool lastSprayOwned = false;
    private bool lastGunOwned = false;

    private void Awake()
    {
        BindReferences();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (inventory != null)
            inventory.OnEquipmentChanged -= OnEquipmentChanged;
    }

    private void Start()
    {
        ApplyOwnershipIcons();
        UpdateSlots(inventory != null ? inventory.GetCurrentEquipment() : Inventory.EquipmentType.None);
        UpdateAmmoDisplay(inventory != null ? inventory.GetCurrentEquipment() : Inventory.EquipmentType.None);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindReferences();
    }

    private void BindReferences()
    {
        if (inventory != null)
            inventory.OnEquipmentChanged -= OnEquipmentChanged;

        inventory = FindFirstObjectByType<Inventory>();
        player = FindFirstObjectByType<PlayerController>();

        if (inventory == null)
        {
            Debug.LogWarning("WeaponUI: No Inventory found.");
            return;
        }

        inventory.OnEquipmentChanged += OnEquipmentChanged;

        parentGroup.alpha = 0f;
        inactivityTimer = 0f;
        targetAlpha = 0f;
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnEquipmentChanged -= OnEquipmentChanged;
    }

    private void OnEquipmentChanged(Inventory.EquipmentType newEquipment)
    {
        UpdateSlots(newEquipment);
        UpdateAmmoDisplay(newEquipment);

        if (newEquipment == Inventory.EquipmentType.None)
        {
            inactivityTimer = 0f;
            targetAlpha = 0f;
            parentGroup.alpha = 0f;
        }
        else
        {
            bool isClimbing = player != null && player.GetCurrentState() is PlayerClimbingState;
            if (!isClimbing && !wasClimbing)
                inactivityTimer = fadeDelay;
        }

        ApplyOwnershipIcons();
    }

    private void Update()
    {
        if (inventory == null) return;

        var current = inventory.GetCurrentEquipment();
        bool isClimbing = player != null && player.GetCurrentState() is PlayerClimbingState;

        if (isClimbing)
        {
            targetAlpha = 0f;
            inactivityTimer = 0f;
            wasClimbing = true;
        }
        else if (wasClimbing)
        {
            wasClimbing = false;
            inactivityTimer = 0f;
            targetAlpha = 0f;
        }
        else if (current == Inventory.EquipmentType.None)
        {
            targetAlpha = 0f;
            inactivityTimer = 0f;
        }
        else
        {
            UpdateAmmoDisplay(current);
            CheckOwnershipChange();

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxis("Mouse ScrollWheel") != 0)
                inactivityTimer = fadeDelay;

            if (inactivityTimer > 0)
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

    private void UpdateSlots(Inventory.EquipmentType current)
    {
        if (current == Inventory.EquipmentType.None) return;
        SetSlotAlpha(spraySlot, current == Inventory.EquipmentType.Spray);
        SetSlotAlpha(gunSlot, current == Inventory.EquipmentType.Gun);
    }

    private void SetSlotAlpha(CanvasGroup slot, bool isEquipped)
    {
        if (slot == null) return;
        slot.alpha = isEquipped ? 1f : dimAlpha;
    }

    private void UpdateAmmoDisplay(Inventory.EquipmentType current)
    {
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