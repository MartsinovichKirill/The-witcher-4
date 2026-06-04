using UnityEngine;
using WitcherRightVersion.Quest;
using WitcherRightVersion.Save;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Core
{
    public sealed class EndingService : MonoBehaviour
    {
        public const string TruthEndingType = "Truth";
        public const string LieEndingType = "Lie";
        public const string SacrificeEndingType = "Sacrifice";
        public const string PendingTruthRouteKey = "ending.pendingTruthRoute";
        public const string CompletedEndingKey = "ending.completedType";
        public const string TruthEndingFlag = "MvpTruthEndingReached";
        public const string LieEndingFlag = "LieEndingReached";
        public const string SacrificeEndingFlag = "SacrificeEndingReached";

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
            return PlayerPrefs.GetInt(PendingTruthRouteKey, 0) == 1
                || DecisionFlagService.Instance != null && DecisionFlagService.Instance.HasFlag(TruthEndingFlag)
                || HasWorldTruthRouteRequirements();
        }

        public bool CanCompleteLieEnding()
        {
            var flags = DecisionFlagService.Instance;
            var quest = QuestService.Instance;

            return flags != null
                && (flags.HasFlag("MayorSupported")
                    || flags.HasFlag(LieEndingFlag)
                    || quest != null
                        && quest.SwampContractState == QuestState.Completed
                        && !flags.HasFlag("questionedElderVersion"));
        }

        public bool CanCompleteSacrificeEnding()
        {
            var flags = DecisionFlagService.Instance;

            return flags != null
                && (flags.HasFlag("OrtenDiaryFound")
                    || flags.HasFlag("MirrorShardsDestroyed")
                    || flags.HasFlag(SacrificeEndingFlag));
        }

        private static bool HasWorldTruthRouteRequirements()
        {
            var quest = QuestService.Instance;
            var flags = DecisionFlagService.Instance;

            return quest != null
                && flags != null
                && quest.SwampContractState == QuestState.Completed
                && flags.HasFlag("questionedElderVersion")
                && (flags.HasFlag("MedallionFound") || flags.HasFlag(TruthEndingFlag));
        }

        public bool CompleteTruthEnding()
        {
            if (!CanCompleteTruthEnding())
            {
                InteractionPromptUI.Instance?.ShowMessage("The altar is silent. Reynard needs stronger proof before choosing the truth.");
                return false;
            }

            return CompleteEnding(
                TruthEndingType,
                TruthEndingFlag,
                "VillageTruthExposed",
                "Ending reached: Truth. The first right version is no longer buried.");
        }

        public bool CompleteLieEnding()
        {
            if (!CanCompleteLieEnding())
            {
                InteractionPromptUI.Instance?.ShowMessage("The mirror stays dark. Reynard needs the elder's version or fewer doubts before choosing the lie.");
                return false;
            }

            return CompleteEnding(
                LieEndingType,
                LieEndingFlag,
                "MayorSupported",
                "Ending reached: Corrected Story. Velemar survives by choosing the useful lie.");
        }

        public bool CompleteSacrificeEnding()
        {
            if (!CanCompleteSacrificeEnding())
            {
                InteractionPromptUI.Instance?.ShowMessage("The shards do not break. Reynard needs Orten's diary or destroyed mirror fragments before choosing sacrifice.");
                return false;
            }

            return CompleteEnding(
                SacrificeEndingType,
                SacrificeEndingFlag,
                "MirrorDestroyed",
                "Ending reached: Sacrifice. The curse breaks, and Velemar pays the living price.");
        }

        public bool CompleteEndingByType(string endingType)
        {
            switch (endingType)
            {
                case TruthEndingType:
                    return CompleteTruthEnding();
                case LieEndingType:
                    return CompleteLieEnding();
                case SacrificeEndingType:
                    return CompleteSacrificeEnding();
                default:
                    InteractionPromptUI.Instance?.ShowMessage("This ending has not been written yet.");
                    return false;
            }
        }

        private bool CompleteEnding(string endingType, string endingFlag, string consequenceFlag, string message)
        {
            CompletedEnding = endingType;
            PlayerPrefs.SetString(CompletedEndingKey, CompletedEnding);
            PlayerPrefs.DeleteKey(PendingTruthRouteKey);
            PlayerPrefs.Save();

            var flags = DecisionFlagService.Instance;
            flags?.SetFlag(endingFlag);
            flags?.SetFlag(consequenceFlag);
            SaveService.Instance?.SaveAutosave();
            EndingHudUI.Instance?.ShowEnding(CompletedEnding);
            InteractionPromptUI.Instance?.ShowMessage(message);
            Debug.Log($"Ending completed: {endingType}", this);
            return true;
        }
    }
}
