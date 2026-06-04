using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using WitcherRightVersion.Combat;
using WitcherRightVersion.Core;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Quest;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Save
{
    public sealed class SaveService : MonoBehaviour
    {
        private const string AutosaveFileName = "autosave.json";
        private const string ManualSlotPrefix = "manual_slot_";
        private const string ManualSlotExtension = ".json";
        private const int ManualSlotCount = 3;
        private const string PendingAutosaveLoadKey = "save.pendingAutosaveLoad";

        public static SaveService Instance { get; private set; }

        public string SaveDirectory => SaveRootDirectory;
        public static string SaveRootDirectory => Path.Combine(Application.persistentDataPath, "WitcherRightVersion");

        private static SaveData pendingSceneTransferData;
        private bool isRestoring;
        private bool questEventsAttached;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            AttachQuestEvents();
        }

        private void Start()
        {
            AttachQuestEvents();

            if (TryApplyPendingSceneTransfer())
            {
                return;
            }

            if (PlayerPrefs.GetInt(PendingAutosaveLoadKey, 0) == 1)
            {
                PlayerPrefs.DeleteKey(PendingAutosaveLoadKey);
                PlayerPrefs.Save();
                LoadAutosave();
            }
        }

        private void OnDisable()
        {
            if (QuestService.Instance != null && questEventsAttached)
            {
                QuestService.Instance.QuestChanged -= HandleQuestChanged;
                questEventsAttached = false;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveManualSlot(1);
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                SaveManualSlot(2);
            }
            else if (Input.GetKeyDown(KeyCode.F7))
            {
                SaveManualSlot(3);
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                LoadAutosave();
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                LoadManualSlot(1);
            }
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                LoadManualSlot(2);
            }
            else if (Input.GetKeyDown(KeyCode.F11))
            {
                LoadManualSlot(3);
            }
        }

        public bool SaveAutosave()
        {
            if (isRestoring)
            {
                return false;
            }

            return SaveToPath(GetAutosavePath(), "Autosave");
        }

        public bool LoadAutosave()
        {
            return LoadFromPath(GetAutosavePath(), "Autosave");
        }

        public bool SaveManualSlot(int slotNumber)
        {
            if (!IsValidManualSlot(slotNumber))
            {
                Debug.LogWarning($"Invalid manual save slot: {slotNumber}", this);
                return false;
            }

            return SaveToPath(GetManualSlotPath(slotNumber), $"Manual slot {slotNumber}");
        }

        public bool LoadManualSlot(int slotNumber)
        {
            if (!IsValidManualSlot(slotNumber))
            {
                Debug.LogWarning($"Invalid manual load slot: {slotNumber}", this);
                return false;
            }

            return LoadFromPath(GetManualSlotPath(slotNumber), $"Manual slot {slotNumber}");
        }

        public static bool HasAutosave()
        {
            return File.Exists(GetStaticAutosavePath());
        }

        public static void RequestAutosaveLoadOnNextScene()
        {
            PlayerPrefs.SetInt(PendingAutosaveLoadKey, 1);
            PlayerPrefs.Save();
        }

        public static bool PrepareSceneTransfer()
        {
            if (Instance == null)
            {
                return false;
            }

            var data = Instance.CaptureSaveData();
            if (data == null)
            {
                return false;
            }

            pendingSceneTransferData = data;
            Debug.Log($"Scene transfer prepared from {data.sceneName}.");
            return true;
        }

        private bool SaveToPath(string path, string label)
        {
            var data = CaptureSaveData();
            if (data == null)
            {
                InteractionPromptUI.Instance?.ShowMessage($"{label} failed: no player.");
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
            Debug.Log($"{label} written to {path}", this);
            InteractionPromptUI.Instance?.ShowMessage($"{label} saved.");
            return true;
        }

        private bool LoadFromPath(string path, string label)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"{label} not found: {path}", this);
                InteractionPromptUI.Instance?.ShowMessage($"{label} not found.");
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null)
                {
                    InteractionPromptUI.Instance?.ShowMessage($"{label} failed: broken save.");
                    return false;
                }

                var currentScene = SceneManager.GetActiveScene().name;
                if (!string.Equals(data.sceneName, currentScene, StringComparison.Ordinal))
                {
                    Debug.LogWarning($"Save scene mismatch. Saved: {data.sceneName}, current: {currentScene}. Cross-scene restore is deferred.", this);
                    InteractionPromptUI.Instance?.ShowMessage($"{label} needs scene {data.sceneName}.");
                    return false;
                }

                RestoreSaveData(data);
                Debug.Log($"{label} loaded from {path}", this);
                InteractionPromptUI.Instance?.ShowMessage($"{label} loaded.");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"{label} load failed: {exception}", this);
                InteractionPromptUI.Instance?.ShowMessage($"{label} failed.");
                return false;
            }
        }

        private SaveData CaptureSaveData()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return null;
            }

            var health = player.GetComponent<Health>();

            return new SaveData
            {
                sceneName = SceneManager.GetActiveScene().name,
                playerPosition = new SerializableVector3(player.transform.position),
                playerHealth = health != null ? health.CurrentHealth : 0f,
                quest = QuestService.Instance != null ? QuestService.Instance.CaptureSnapshot() : null,
                completedQuestInteractables = CaptureCompletedQuestInteractables(),
                decisionFlags = DecisionFlagService.Instance != null ? DecisionFlagService.Instance.CaptureFlags() : Array.Empty<string>(),
                rewards = PlayerRewardService.Instance != null ? PlayerRewardService.Instance.CaptureSnapshot() : null,
                inventory = InventoryService.Instance != null ? InventoryService.Instance.CaptureSnapshot() : null
            };
        }

        private void RestoreSaveData(SaveData data)
        {
            isRestoring = true;
            try
            {
                DecisionFlagService.Instance?.RestoreFlags(data.decisionFlags);
                PlayerRewardService.Instance?.RestoreSnapshot(data.rewards);
                InventoryService.Instance?.RestoreSnapshot(data.inventory);
                RestoreCompletedQuestInteractables(data.completedQuestInteractables);
                QuestService.Instance?.RestoreSnapshot(data.quest);

                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var controller = player.GetComponent<CharacterController>();
                    if (controller != null)
                    {
                        controller.enabled = false;
                    }

                    player.transform.position = data.playerPosition.ToVector3();

                    if (controller != null)
                    {
                        controller.enabled = true;
                    }

                    var health = player.GetComponent<Health>();
                    health?.RestoreCurrentHealth(data.playerHealth);
                }
            }
            finally
            {
                isRestoring = false;
            }
        }

        private bool TryApplyPendingSceneTransfer()
        {
            if (pendingSceneTransferData == null)
            {
                return false;
            }

            var data = pendingSceneTransferData;
            pendingSceneTransferData = null;

            RestoreSharedState(data);
            Debug.Log($"Scene transfer restored from {data.sceneName} to {SceneManager.GetActiveScene().name}.", this);
            InteractionPromptUI.Instance?.ShowMessage("Progress carried into this area.");
            return true;
        }

        private void RestoreSharedState(SaveData data)
        {
            isRestoring = true;
            try
            {
                DecisionFlagService.Instance?.RestoreFlags(data.decisionFlags);
                PlayerRewardService.Instance?.RestoreSnapshot(data.rewards);
                InventoryService.Instance?.RestoreSnapshot(data.inventory);
                RestoreCompletedQuestInteractables(data.completedQuestInteractables);
                QuestService.Instance?.RestoreSnapshot(data.quest);

                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var health = player.GetComponent<Health>();
                    health?.RestoreCurrentHealth(data.playerHealth);
                }
            }
            finally
            {
                isRestoring = false;
            }
        }

        private void AttachQuestEvents()
        {
            if (questEventsAttached || QuestService.Instance == null)
            {
                return;
            }

            QuestService.Instance.QuestChanged += HandleQuestChanged;
            questEventsAttached = true;
        }

        private void HandleQuestChanged()
        {
            SaveAutosave();
        }

        private static string[] CaptureCompletedQuestInteractables()
        {
            var interactables = FindObjectsByType<QuestProgressInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var completedIds = new System.Collections.Generic.List<string>();

            for (var i = 0; i < interactables.Length; i++)
            {
                var interactable = interactables[i];
                if (interactable != null && interactable.IsCompleted && !string.IsNullOrWhiteSpace(interactable.PersistentId))
                {
                    completedIds.Add(interactable.PersistentId);
                }
            }

            return completedIds.ToArray();
        }

        private static void RestoreCompletedQuestInteractables(string[] completedIds)
        {
            var completedSet = new System.Collections.Generic.HashSet<string>(completedIds ?? Array.Empty<string>());
            var interactables = FindObjectsByType<QuestProgressInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (var i = 0; i < interactables.Length; i++)
            {
                var interactable = interactables[i];
                if (interactable != null && !string.IsNullOrWhiteSpace(interactable.PersistentId))
                {
                    interactable.RestoreCompletedState(completedSet.Contains(interactable.PersistentId));
                }
            }
        }

        private string GetAutosavePath()
        {
            return GetStaticAutosavePath();
        }

        private string GetManualSlotPath(int slotNumber)
        {
            return Path.Combine(SaveDirectory, ManualSlotPrefix + slotNumber + ManualSlotExtension);
        }

        private static bool IsValidManualSlot(int slotNumber)
        {
            return slotNumber >= 1 && slotNumber <= ManualSlotCount;
        }

        private static string GetStaticAutosavePath()
        {
            return Path.Combine(SaveRootDirectory, AutosaveFileName);
        }
    }
}
