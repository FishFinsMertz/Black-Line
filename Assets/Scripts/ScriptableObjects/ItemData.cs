using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Save / Logic")]
    public string itemID;

    [Header("Display")]
    public string itemName;
    public Sprite itemIcon;
    [TextArea] public string description;
    public float displayDuration = 4f;
}