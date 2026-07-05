using UnityEngine;

public class ItemGiver : MonoBehaviour
{
    [SerializeField] private string itemID;

    // Public API – call this from a trigger, button, or event
    public void GiveAccessItem()
    {
        if (string.IsNullOrEmpty(itemID))
        {
            Debug.LogWarning($"ItemGiver: {name} has no itemID assigned.");
            return;
        }

        GameData data = SaveManager.Instance?.GetCurrentData();

        if (!data.collectedAccessItems.Contains(itemID))
        {
            data.collectedAccessItems.Add(itemID);
            Debug.Log($"ItemGiver: '{itemID}' added to collectedAccessItems.");
        }
        else
        {
            Debug.Log($"ItemGiver: '{itemID}' already collected.");
        }
    }
}