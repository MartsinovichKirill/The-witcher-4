using UnityEngine;
using WitcherRightVersion.Save;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Core
{
    public sealed class EndingService : MonoBehaviour
    {
        public const string TruthEndingType = "Truth";
        public const string PendingTruthRouteKey = "ending.pendingTruthRoute";
        public const string CompletedEndingKey = "ending.completedType";
        public const string TruthEndingFlag = "MvpTruthEndingReached";

        public static EndingService Instance { get; private set; }

        public string CompletedEnding { get; private set; }
        public bool HasCompletedEnding => !string.IsNullOrWhiteSpace(CompletedEnding);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CompletedEnding = PlayerPrefs.GetString(CompletedEndingKey, string.Empty);
        }

        public static void UnlockTruthRoute()
        {
            PlayerPrefs.SetInt(PendingTruthRouteKey, 1);
            PlayerPrefs.Save();
        }

        public bool CanCompleteTruthEnding()
        {
            return PlayerPrefs.GetInt(PendingTruthRouteKey, 0) == 1 || DecisionFlagService.Instance != null && DecisionFlagService.Instance.HasFlag(TruthEndingFlag);
        }

        public bool CompleteTruthEnding()
        {
            if (!CanCompleteTruthEnding())
            {
                InteractionPromptUI.Instance?.ShowMessage("The altar is silent. Reynard needs stronger proof before choosing the truth.");
                return false;
            }

            CompletedEnding = TruthEndingType;
            PlayerPrefs.SetString(CompletedEndingKey, CompletedEnding);
            PlayerPrefs.DeleteKey(PendingTruthRouteKey);
            PlayerPrefs.Save();

            DecisionFlagService.Instance?.SetFlag(TruthEndingFlag);
            DecisionFlagService.Instance?.SetFlag("VillageTruthExposed");
            SaveService.Instance?.SaveAutosave();
            InteractionPromptUI.Instance?.ShowMessage("Ending reached: Truth. The first right version is no longer buried.");
            Debug.Log("Ending completed: Truth", this);
            return true;
        }
    }
}
