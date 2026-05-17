using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    // Inventory data
    public List<string> ownedItems = new List<string>();
    public string currentEquipment;

    // Future expansions (example)
    // public float playerHealth;
    // public float playerTemperature;
    // public string currentScene;
    // public Vector3 playerPosition;
    // public List<string> unlockedDoors;
    // public List<string> deadEnemies;
}