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
        public string version = "mvp-scene-transfer-0.17";
        public string sceneName;
        public SerializableVector3 playerPosition;
        public float playerHealth;
        public QuestSnapshot quest;
        public string[] completedQuestInteractables;
        public string[] decisionFlags;
        public PlayerRewardSnapshot rewards;
        public InventorySnapshot inventory;
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
