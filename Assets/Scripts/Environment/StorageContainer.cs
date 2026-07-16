using UnityEngine;
using System.Collections;
using System.Linq;

public class StorageContainer : MonoBehaviour, IInteractible, ISaveable
{
    [Header("Save ID")]
    [SerializeField] private string saveID;

    [Header("Container Settings")]
    [SerializeField] private float openDuration = 0.5f;
    [SerializeField] private float closeDuration = 0f;
    [SerializeField] private bool startOpen = false;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;

    [Header("References")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private GameObject lootObject;

    private bool isOpen = false;
    private bool interactible = true;
    private bool isAnimating = false;

    private void Start()
    {
        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Register(this);

        isOpen = startOpen;
        if (doorAnimator != null)
            doorAnimator.Play(startOpen ? "Open" : "Closed", 0, 0f);

        UpdateLootInteraction();
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Unregister(this);
    }

    private void UpdateLootInteraction()
    {
        if (lootObject == null) return;
        ItemGiver giver = lootObject.GetComponent<ItemGiver>();
        if (giver == null) return;

        if (isOpen && !giver.WasGiven)
            giver.EnableInteraction();
        else
            giver.DisableInteraction();
    }

    public void Open()
    {
        if (!interactible || isOpen || isAnimating) return;
        StartCoroutine(OpenSequence());
    }

    private IEnumerator OpenSequence()
    {
        isAnimating = true;
        interactible = false;

        if (openSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShot(openSound, transform.position);
            
        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");

        yield return new WaitForSeconds(openDuration);

        isOpen = true;
        UpdateLootInteraction();
        isAnimating = false;
        interactible = true;
    }

    public void Close()
    {
        if (!interactible || !isOpen || isAnimating) return;
        StartCoroutine(CloseSequence());
    }

    private IEnumerator CloseSequence()
    {
        isAnimating = true;
        interactible = false;

        isOpen = false;
        UpdateLootInteraction();

        if (doorAnimator != null)
            doorAnimator.SetTrigger("Close");

        yield return new WaitForSeconds(closeDuration);

        isAnimating = false;
        interactible = true;
    }

    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    public void EnableInteraction() => interactible = true;
    public void DisableInteraction() => interactible = false;
    public bool IsOpen() => isOpen;

    public void Save(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = isOpen ? "Open" : "Closed" });
    }

    public void Load(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            isOpen = cs.state == "Open";
            UpdateLootInteraction();
            if (doorAnimator != null)
                doorAnimator.Play(isOpen ? "Open" : "Closed", 0, 0f);
        }
    }
}