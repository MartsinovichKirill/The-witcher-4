using System;
using UnityEngine;
using WitcherRightVersion.Core;

namespace WitcherRightVersion.Quest
{
    public sealed class QuestService : MonoBehaviour
    {
        private const int RequiredSwampTraceCount = 3;
        private const int RequiredMissingHunterClueCount = 2;

        public const string SwampContractQuestId = "contract_swamp_beast";
        public const string MissingHunterQuestId = "missing_hunter";
        public const string ActionStartSwampContract = "start_swamp_contract";
        public const string ActionMartaSpoken = "marta_spoken";
        public const string ActionSwampTracesFound = "swamp_traces_found";
        public const string ActionFirstDrownerKilled = "first_drowner_killed";
        public const string ActionReturnedToElder = "returned_to_elder";
        public const string ActionAcceptedElderVersion = "accepted_elder_version";
        public const string ActionQuestionedElderVersion = "questioned_elder_version";
        public const string ActionRewardReceived = "swamp_contract_reward_received";
        public const string ActionStartMissingHunter = "start_missing_hunter";
        public const string ActionMissingHunterClueFound = "missing_hunter_clue_found";
        public const string ActionMissingHunterReturned = "missing_hunter_returned";
        public const string ActionMissingHunterRewardReceived = "missing_hunter_reward_received";

        private QuestState swampContractState = QuestState.NotStarted;
        private SwampContractStage swampContractStage = SwampContractStage.TalkToElder;
        private int swampTraceCount;
        private QuestState missingHunterState = QuestState.NotStarted;
        private MissingHunterStage missingHunterStage = MissingHunterStage.FindClues;
        private int missingHunterClueCount;

        public static QuestService Instance { get; private set; }
        public event Action QuestChanged;

        public QuestState SwampContractState => swampContractState;
        public SwampContractStage CurrentSwampContractStage => swampContractStage;
        public int SwampTraceCount => swampTraceCount;
        public int SwampTraceTarget => RequiredSwampTraceCount;
        public QuestState MissingHunterState => missingHunterState;
        public MissingHunterStage CurrentMissingHunterStage => missingHunterStage;
        public int MissingHunterClueCount => missingHunterClueCount;
        public int MissingHunterClueTarget => RequiredMissingHunterClueCount;
        public bool HasActiveQuest => swampContractState == QuestState.Active
            || missingHunterState == QuestState.Active
            || swampContractState == QuestState.Completed
            || missingHunterState == QuestState.Completed;

        public string ActiveQuestTitle
        {
            get
            {
                if (swampContractState == QuestState.Active)
                {
                    return "Contract: Beast from the Swamp";
                }

                if (missingHunterState == QuestState.Active)
                {
                    return "Missing Hunter";
                }

                if (missingHunterState == QuestState.Completed)
                {
                    return "Missing Hunter";
                }

                return swampContractState == QuestState.Completed ? "Contract: Beast from the Swamp" : string.Empty;
            }
        }

        public string CurrentObjective
        {
            get
            {
                if (swampContractState == QuestState.Active)
                {
                    return GetSwampContractObjective();
                }

                if (missingHunterState == QuestState.Active || missingHunterState == QuestState.Completed)
                {
                    return GetMissingHunterObjective();
                }

                return swampContractState == QuestState.Completed ? GetSwampContractObjective() : string.Empty;
            }
        }

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
                    return RecordSwampTrace();
                case ActionFirstDrownerKilled:
                    return AdvanceSwampContract(SwampContractStage.KillDrowner, SwampContractStage.ReturnToElder);
                case ActionReturnedToElder:
                    return AdvanceSwampContract(SwampContractStage.ReturnToElder, SwampContractStage.ChooseResponse);
                case ActionAcceptedElderVersion:
                    return AdvanceSwampContract(SwampContractStage.ChooseResponse, SwampContractStage.ReceiveReward);
                case ActionQuestionedElderVersion:
                    return AdvanceSwampContract(SwampContractStage.ChooseResponse, SwampContractStage.ReceiveReward);
                case ActionRewardReceived:
                    return CompleteSwampContract();
                case ActionStartMissingHunter:
                    return StartMissingHunter();
                case ActionMissingHunterClueFound:
                    return RecordMissingHunterClue();
                case ActionMissingHunterReturned:
                    return ReturnAndCompleteMissingHunter();
                case ActionMissingHunterRewardReceived:
                    return CompleteMissingHunter();
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
            swampTraceCount = 0;
            NotifyQuestChanged("Quest started");
            return true;
        }

        private bool RecordSwampTrace()
        {
            if (swampContractState != QuestState.Active)
            {
                Debug.Log($"Cannot record swamp trace: quest is {swampContractState}.", this);
                return false;
            }

            if (swampContractStage != SwampContractStage.FindSwampTraces)
            {
                Debug.Log($"Cannot record swamp trace: current stage is {swampContractStage}.", this);
                return false;
            }

            swampTraceCount = Mathf.Clamp(swampTraceCount + 1, 0, RequiredSwampTraceCount);
            if (swampTraceCount >= RequiredSwampTraceCount)
            {
                swampContractStage = SwampContractStage.KillDrowner;
                NotifyQuestChanged("Trace investigation complete");
                return true;
            }

            NotifyQuestChanged("Swamp trace found");
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
            PlayerRewardService.Instance?.GrantSwampContractReward();
            NotifyQuestChanged("Quest completed");
            return true;
        }

        private bool StartMissingHunter()
        {
            if (missingHunterState != QuestState.NotStarted)
            {
                Debug.Log($"Quest already started: {MissingHunterQuestId}", this);
                return false;
            }

            missingHunterState = QuestState.Active;
            missingHunterStage = MissingHunterStage.FindClues;
            missingHunterClueCount = 0;
            DecisionFlagService.Instance?.SetFlag("missingHunterStarted");
            NotifyQuestChanged("Quest started", MissingHunterQuestId, missingHunterStage.ToString());
            return true;
        }

        private bool RecordMissingHunterClue()
        {
            if (missingHunterState != QuestState.Active)
            {
                Debug.Log($"Cannot record hunter clue: quest is {missingHunterState}.", this);
                return false;
            }

            if (missingHunterStage != MissingHunterStage.FindClues)
            {
                Debug.Log($"Cannot record hunter clue: current stage is {missingHunterStage}.", this);
                return false;
            }

            missingHunterClueCount = Mathf.Clamp(missingHunterClueCount + 1, 0, RequiredMissingHunterClueCount);
            if (missingHunterClueCount >= RequiredMissingHunterClueCount)
            {
                missingHunterStage = MissingHunterStage.ReturnToCamp;
                NotifyQuestChanged("Hunter clues complete", MissingHunterQuestId, missingHunterStage.ToString());
                return true;
            }

            NotifyQuestChanged("Hunter clue found", MissingHunterQuestId, missingHunterStage.ToString());
            return true;
        }

        private bool AdvanceMissingHunter(MissingHunterStage expectedStage, MissingHunterStage nextStage)
        {
            if (missingHunterState != QuestState.Active)
            {
                Debug.Log($"Cannot advance {MissingHunterQuestId}: quest is {missingHunterState}.", this);
                return false;
            }

            if (missingHunterStage != expectedStage)
            {
                Debug.Log($"Cannot advance {MissingHunterQuestId}: expected {expectedStage}, current {missingHunterStage}.", this);
                return false;
            }

            missingHunterStage = nextStage;
            NotifyQuestChanged("Quest advanced", MissingHunterQuestId, missingHunterStage.ToString());
            return true;
        }

        private bool CompleteMissingHunter()
        {
            if (missingHunterState != QuestState.Active || missingHunterStage != MissingHunterStage.ReceiveReward)
            {
                Debug.Log($"Cannot complete {MissingHunterQuestId}: current stage is {missingHunterStage}.", this);
                return false;
            }

            missingHunterState = QuestState.Completed;
            missingHunterStage = MissingHunterStage.Completed;
            PlayerRewardService.Instance?.GrantMissingHunterReward();
            NotifyQuestChanged("Quest completed", MissingHunterQuestId, missingHunterStage.ToString());
            return true;
        }

        private bool ReturnAndCompleteMissingHunter()
        {
            if (!AdvanceMissingHunter(MissingHunterStage.ReturnToCamp, MissingHunterStage.ReceiveReward))
            {
                return false;
            }

            return CompleteMissingHunter();
        }

        private string GetSwampContractObjective()
        {
            switch (swampContractStage)
            {
                case SwampContractStage.SpeakWithMarta:
                    return "Speak with Marta about swamp poison.";
                case SwampContractStage.FindSwampTraces:
                    return $"Inspect swamp traces south of the village ({swampTraceCount}/{RequiredSwampTraceCount}).";
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

        private string GetMissingHunterObjective()
        {
            switch (missingHunterStage)
            {
                case MissingHunterStage.FindClues:
                    return $"Search the Old Forest for hunter signs ({missingHunterClueCount}/{RequiredMissingHunterClueCount}).";
                case MissingHunterStage.ReturnToCamp:
                    return "Return to the hunter camp and decide what the signs mean.";
                case MissingHunterStage.ReceiveReward:
                    return "Take the hunter's emergency pouch: 25 XP and 10 coins.";
                case MissingHunterStage.Completed:
                    return "Missing Hunter completed.";
                default:
                    return "Search the Old Forest.";
            }
        }

        private void NotifyQuestChanged(string reason)
        {
            NotifyQuestChanged(reason, SwampContractQuestId, swampContractStage.ToString());
        }

        private void NotifyQuestChanged(string reason, string questId, string stage)
        {
            Debug.Log($"{reason}: {questId} -> {stage}", this);
            AudioFeedbackService.Instance?.PlayQuest();
            QuestChanged?.Invoke();
        }

        public QuestSnapshot CaptureSnapshot()
        {
            return new QuestSnapshot
            {
                swampContractState = swampContractState.ToString(),
                swampContractStage = swampContractStage.ToString(),
                swampTraceCount = swampTraceCount,
                missingHunterState = missingHunterState.ToString(),
                missingHunterStage = missingHunterStage.ToString(),
                missingHunterClueCount = missingHunterClueCount
            };
        }

        public void RestoreSnapshot(QuestSnapshot snapshot)
        {
            if (snapshot == null)
            {
                swampContractState = QuestState.NotStarted;
                swampContractStage = SwampContractStage.TalkToElder;
                swampTraceCount = 0;
                missingHunterState = QuestState.NotStarted;
                missingHunterStage = MissingHunterStage.FindClues;
                missingHunterClueCount = 0;
                NotifyQuestChanged("Quest restored");
                return;
            }

            if (!Enum.TryParse(snapshot.swampContractState, out swampContractState))
            {
                swampContractState = QuestState.NotStarted;
            }

            if (!Enum.TryParse(snapshot.swampContractStage, out swampContractStage))
            {
                swampContractStage = SwampContractStage.TalkToElder;
            }

            swampTraceCount = Mathf.Clamp(snapshot.swampTraceCount, 0, RequiredSwampTraceCount);
            if (!Enum.TryParse(snapshot.missingHunterState, out missingHunterState))
            {
                missingHunterState = QuestState.NotStarted;
            }

            if (!Enum.TryParse(snapshot.missingHunterStage, out missingHunterStage))
            {
                missingHunterStage = MissingHunterStage.FindClues;
            }

            missingHunterClueCount = Mathf.Clamp(snapshot.missingHunterClueCount, 0, RequiredMissingHunterClueCount);
            NotifyQuestChanged("Quest restored");
        }
    }

    [Serializable]
    public sealed class QuestSnapshot
    {
        public string swampContractState;
        public string swampContractStage;
        public int swampTraceCount;
        public string missingHunterState;
        public string missingHunterStage;
        public int missingHunterClueCount;
    }
}
