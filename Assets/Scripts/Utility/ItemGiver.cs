using UnityEngine;

public class ItemGiver : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    public void GiveAccessItem()
    {
        if (itemData == null)
        {
            Debug.LogWarning($"ItemGiver: {name} has no ItemData assigned.");
            return;
        }

        if (string.IsNullOrEmpty(itemData.itemID))
        {
            Debug.LogWarning($"ItemGiver: {name} has empty itemID in ItemData.");
            return;
        }

        GameData data = SaveManager.Instance?.GetCurrentData();
        if (data == null) return;

        if (!data.collectedAccessItems.Contains(itemData.itemID))
        {
            data.collectedAccessItems.Add(itemData.itemID);
            Debug.Log($"ItemGiver: '{itemData.itemID}' added to collectedAccessItems.");

            if (NotificationManager.Instance != null)
                NotificationManager.Instance.ShowItemNotification(itemData);
        }
        else
        {
            Debug.Log($"ItemGiver: '{itemData.itemID}' already collected.");
        }
    }
}