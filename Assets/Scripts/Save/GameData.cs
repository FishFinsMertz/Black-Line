using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    // Inventory data
    public List<string> ownedItems = new List<string>();
    public string currentEquipment;
    public float playerBaseTemperature = 70f;
    public List<string> deadEnemyIDs = new List<string>();
    public float playerBatteryAmt = 100f;

    // Future expansions (example)
    // public string currentScene;
    // public Vector3 playerPosition;
    // public List<string> unlockedDoors;
}