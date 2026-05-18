using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    // Inventory data
    public List<string> ownedItems = new List<string>();
    public string currentEquipment;

    public float playerBaseTemperature = 70f;

    // Future expansions (example)
    // public string currentScene;
    // public Vector3 playerPosition;
    // public List<string> unlockedDoors;
    // public List<string> deadEnemies;
}