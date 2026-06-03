using System;
using UnityEngine;

namespace WitcherRightVersion.Quest
{
    public sealed class QuestService : MonoBehaviour
    {
        public const string SwampContractQuestId = "contract_swamp_beast";
        public const string ActionStartSwampContract = "start_swamp_contract";
        public const string ActionMartaSpoken = "marta_spoken";
        public const string ActionSwampTracesFound = "swamp_traces_found";
        public const string ActionFirstDrownerKilled = "first_drowner_killed";
        public const string ActionReturnedToElder = "returned_to_elder";
        public const string ActionQuestionedElderVersion = "questioned_elder_version";
        public const string ActionRewardReceived = "swamp_contract_reward_received";

        private QuestState swampContractState = QuestState.NotStarted;
        private SwampContractStage swampContractStage = SwampContractStage.TalkToElder;

        public static QuestService Instance { get; private set; }
        public event Action QuestChanged;

        public QuestState SwampContractState => swampContractState;
        public SwampContractStage CurrentSwampContractStage => swampContractStage;
        public bool HasActiveQuest => swampContractState == QuestState.Active || swampContractState == QuestState.Completed;

        public string ActiveQuestTitle => HasActiveQuest ? "Contract: Beast from the Swamp" : string.Empty;
        public string CurrentObjective => HasActiveQuest ? GetSwampContractObjective() : string.Empty;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public bool RunAction(string actionId)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                return false;
            }

            switch (actionId)
            {
                case ActionStartSwampContract:
                    return StartSwampContract();
                case ActionMartaSpoken:
                    return AdvanceSwampContract(SwampContractStage.SpeakWithMarta, SwampContractStage.FindSwampTraces);
                case ActionSwampTracesFound:
                    return AdvanceSwampContract(SwampContractStage.FindSwampTraces, SwampContractStage.KillDrowner);
                case ActionFirstDrownerKilled:
                    return AdvanceSwampContract(SwampContractStage.KillDrowner, SwampContractStage.ReturnToElder);
                case ActionReturnedToElder:
                    return AdvanceSwampContract(SwampContractStage.ReturnToElder, SwampContractStage.ChooseResponse);
                case ActionQuestionedElderVersion:
                    return AdvanceSwampContract(SwampContractStage.ChooseResponse, SwampContractStage.ReceiveReward);
                case ActionRewardReceived:
                    return CompleteSwampContract();
                default:
                    Debug.LogWarning($"Unknown quest action: {actionId}", this);
                    return false;
            }
        }

        private bool StartSwampContract()
        {
            if (swampContractState != QuestState.NotStarted)
            {
                Debug.Log($"Quest already started: {SwampContractQuestId}", this);
                return false;
            }

            swampContractState = QuestState.Active;
            swampContractStage = SwampContractStage.SpeakWithMarta;
            NotifyQuestChanged("Quest started");
            return true;
        }

        private bool AdvanceSwampContract(SwampContractStage expectedStage, SwampContractStage nextStage)
        {
            if (swampContractState != QuestState.Active)
            {
                Debug.Log($"Cannot advance {SwampContractQuestId}: quest is {swampContractState}.", this);
                return false;
            }

            if (swampContractStage != expectedStage)
            {
                Debug.Log($"Cannot advance {SwampContractQuestId}: expected {expectedStage}, current {swampContractStage}.", this);
                return false;
            }

            swampContractStage = nextStage;
            NotifyQuestChanged("Quest advanced");
            return true;
        }

        private bool CompleteSwampContract()
        {
            if (swampContractState != QuestState.Active || swampContractStage != SwampContractStage.ReceiveReward)
            {
                Debug.Log($"Cannot complete {SwampContractQuestId}: current stage is {swampContractStage}.", this);
                return false;
            }

            swampContractState = QuestState.Completed;
            swampContractStage = SwampContractStage.Completed;
            NotifyQuestChanged("Quest completed");
            return true;
        }

        private string GetSwampContractObjective()
        {
            switch (swampContractStage)
            {
                case SwampContractStage.SpeakWithMarta:
                    return "Speak with Marta about swamp poison.";
                case SwampContractStage.FindSwampTraces:
                    return "Inspect the muddy tracks south of the village.";
                case SwampContractStage.KillDrowner:
                    return "Kill the drowner near the swamp road.";
                case SwampContractStage.ReturnToElder:
                    return "Return to Elder Voytsekh with proof.";
                case SwampContractStage.ChooseResponse:
                    return "Choose what to tell the elder.";
                case SwampContractStage.ReceiveReward:
                    return "Receive 50 XP, 20 coins, and the Antitoxin recipe.";
                case SwampContractStage.Completed:
                    return "Contract completed.";
                default:
                    return "Talk to Elder Voytsekh.";
            }
        }

        private void NotifyQuestChanged(string reason)
        {
            Debug.Log($"{reason}: {SwampContractQuestId} -> {swampContractStage}", this);
            QuestChanged?.Invoke();
        }
    }
}
