using System;
using System.Collections.Generic;

[Serializable]
public class ComponentState
{
    public string id;      // unique identifier for the component (set in Inspector)
    public string state;   // flexible string – e.g., "Open", "Closed", "Active", "Inactive", "Used", "90"
}

[System.Serializable]
public class AmmoEntry
{
    public string weaponID;
    public int magazine;
    public int reserve;
}

[Serializable]
public class GameData
{
    // Player data
    public List<string> ownedItems = new List<string>();
    public string currentEquipment;
    public float playerBaseTemperature = 70f;
    public float playerBatteryAmt = 100f;

    // Ammo
    public List<AmmoEntry> weaponAmmo = new List<AmmoEntry>();

    // Collectibles
    public List<string> collectedAccessItems = new List<string>();

    // Enemies
    public List<string> deadEnemyIDs = new List<string>();

    // Environmental states
    public List<ComponentState> componentStates = new List<ComponentState>();

    // Future expansions (uncomment as needed)
    // public string currentScene;
    // public Vector3 playerPosition;
}