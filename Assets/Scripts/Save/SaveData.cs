using System;
using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Quest;

namespace WitcherRightVersion.Save
{
    [Serializable]
    public sealed class SaveData
    {
        public string saveVersion = "pz-structure-0.22";
        public string currentScene;
        public SerializableVector3 playerPosition;
        public float playerHealth;
        public int playerLevel;
        public int playerExperience;
        public int skillPoints;
        public int coins;
        public string equippedWeapon;
        public QuestSnapshot quest;
        public string[] completedQuestInteractables;
        public string[] decisionFlags;
        public PlayerRewardSnapshot rewards;
        public InventorySnapshot inventory;
        public LocationData[] locations;
        public SettingsData settings;
    }

    [Serializable]
    public sealed class LocationData
    {
        public string locationId;
        public string locationName;
        public bool isUnlocked;
        public bool visited;
    }

    [Serializable]
    public sealed class SettingsData
    {
        public string language;
        public float volume;
        public float musicVolume;
        public string resolution;
        public string graphicsQuality;
    }

    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
