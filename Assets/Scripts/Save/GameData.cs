using System;
using System.Collections.Generic;
using UnityEngine;

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
public class ConsumableEntry
{
    public string id;
    public int amount;
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

    // Consumables
    public List<ConsumableEntry> consumables = new List<ConsumableEntry>();

    // Position
    public string currentScene;
    public Vector2 playerPosition;

    // Collectibles
    public List<string> collectedAccessItems = new List<string>();

    // Enemies
    public List<string> deadEnemyIDs = new List<string>();

    // Environmental states
    public List<ComponentState> componentStates = new List<ComponentState>();

}