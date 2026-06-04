using System;
using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.Inventory;

namespace WitcherRightVersion.Quest
{
    public sealed class QuestService : MonoBehaviour
    {
        private const int RequiredSwampTraceCount = 3;
        private const int RequiredMissingHunterClueCount = 2;

        public const string SwampContractQuestId = "contract_swamp_beast";
        public const string RightVersionQuestId = "right_version";
        public const string MirrorTruthQuestId = "mirror_truth";
        public const string MissingHunterQuestId = "missing_hunter";
        public const string SmithDebtQuestId = "smith_debt";
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
        public const string ActionStartSmithDebt = "start_smith_debt";
        public const string ActionOldCampBladeFound = "old_camp_blade_found";
        public const string ActionSmithDebtReturned = "smith_debt_returned";
        public const string ActionStartRightVersion = "start_right_version";
        public const string ActionElsaProtected = "elsa_protected";
        public const string ActionElsaBetrayed = "elsa_betrayed";
        public const string ActionMedallionFound = "medallion_found";
        public const string ActionTowerRouteOpened = "tower_route_opened";
        public const string ActionOrtenDiaryFound = "orten_diary_found";
        public const string ActionOrtenConfronted = "orten_confronted";
        public const string ActionMirrorShardsDestroyed = "mirror_shards_destroyed";

        private QuestState swampContractState = QuestState.NotStarted;
        private SwampContractStage swampContractStage = SwampContractStage.TalkToElder;
        private int swampTraceCount;
        private QuestState rightVersionState = QuestState.NotStarted;
        private RightVersionStage rightVersionStage = RightVersionStage.FindElsa;
        private QuestState mirrorTruthState = QuestState.NotStarted;
        private MirrorTruthStage mirrorTruthStage = MirrorTruthStage.EnterTower;
        private QuestState missingHunterState = QuestState.NotStarted;
        private MissingHunterStage missingHunterStage = MissingHunterStage.FindClues;
        private int missingHunterClueCount;
        private QuestState smithDebtState = QuestState.NotStarted;
        private SmithDebtStage smithDebtStage = SmithDebtStage.FindOldCampBlade;

        public static QuestService Instance { get; private set; }
        public event Action QuestChanged;

        public QuestState SwampContractState => swampContractState;
        public SwampContractStage CurrentSwampContractStage => swampContractStage;
        public int SwampTraceCount => swampTraceCount;
        public int SwampTraceTarget => RequiredSwampTraceCount;
        public QuestState RightVersionState => rightVersionState;
        public RightVersionStage CurrentRightVersionStage => rightVersionStage;
        public QuestState MirrorTruthState => mirrorTruthState;
        public MirrorTruthStage CurrentMirrorTruthStage => mirrorTruthStage;
        public QuestState MissingHunterState => missingHunterState;
        public MissingHunterStage CurrentMissingHunterStage => missingHunterStage;
        public int MissingHunterClueCount => missingHunterClueCount;
        public int MissingHunterClueTarget => RequiredMissingHunterClueCount;
        public QuestState SmithDebtState => smithDebtState;
        public SmithDebtStage CurrentSmithDebtStage => smithDebtStage;
        public bool HasActiveQuest => swampContractState == QuestState.Active
            || rightVersionState == QuestState.Active
            || mirrorTruthState == QuestState.Active
            || missingHunterState == QuestState.Active
            || smithDebtState == QuestState.Active
            || swampContractState == QuestState.Completed
            || rightVersionState == QuestState.Completed
            || mirrorTruthState == QuestState.Completed
            || missingHunterState == QuestState.Completed
            || smithDebtState == QuestState.Completed;

        public string ActiveQuestTitle
        {
            get
            {
                if (mirrorTruthState == QuestState.Active || mirrorTruthState == QuestState.Completed)
                {
                    return "Mirror of Truth";
                }

                if (rightVersionState == QuestState.Active || rightVersionState == QuestState.Completed)
                {
                    return "Right Version";
                }

                if (swampContractState == QuestState.Active)
                {
                    return "Contract: Beast from the Swamp";
                }

                if (missingHunterState == QuestState.Active)
                {
                    return "Missing Hunter";
                }

                if (smithDebtState == QuestState.Active)
                {
                    return "Smith's Debt";
                }

                if (missingHunterState == QuestState.Completed)
                {
                    return "Missing Hunter";
                }

                if (smithDebtState == QuestState.Completed)
                {
                    return "Smith's Debt";
                }

                return swampContractState == QuestState.Completed ? "Contract: Beast from the Swamp" : string.Empty;
            }
        }

        public string CurrentObjective
        {
            get
            {
                if (mirrorTruthState == QuestState.Active || mirrorTruthState == QuestState.Completed)
                {
                    return GetMirrorTruthObjective();
                }

                if (rightVersionState == QuestState.Active || rightVersionState == QuestState.Completed)
                {
                    return GetRightVersionObjective();
                }

                if (swampContractState == QuestState.Active)
                {
                    return GetSwampContractObjective();
                }

                if (missingHunterState == QuestState.Active || missingHunterState == QuestState.Completed)
                {
                    return GetMissingHunterObjective();
                }

                if (smithDebtState == QuestState.Active || smithDebtState == QuestState.Completed)
                {
                    return GetSmithDebtObjective();
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
                case ActionStartSmithDebt:
                    return StartSmithDebt();
                case ActionOldCampBladeFound:
                    return FindOldCampBlade();
                case ActionSmithDebtReturned:
                    return ReturnAndCompleteSmithDebt();
                case ActionStartRightVersion:
                    return StartRightVersion();
                case ActionElsaProtected:
                    return ResolveElsa(true);
                case ActionElsaBetrayed:
                    return ResolveElsa(false);
                case ActionMedallionFound:
                    return RecordMedallionFound();
                case ActionTowerRouteOpened:
                    return OpenTowerRoute();
                case ActionOrtenDiaryFound:
                    return RecordOrtenDiary();
                case ActionOrtenConfronted:
                    return ConfrontOrten();
                case ActionMirrorShardsDestroyed:
                    return DestroyMirrorShards();
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

        private bool StartRightVersion()
        {
            if (rightVersionState != QuestState.NotStarted)
            {
                Debug.Log($"Quest already started: {RightVersionQuestId}", this);
                return false;
            }

            rightVersionState = QuestState.Active;
            rightVersionStage = RightVersionStage.FindElsa;
            DecisionFlagService.Instance?.SetFlag("rightVersionStarted");
            NotifyQuestChanged("Quest started", RightVersionQuestId, rightVersionStage.ToString());
            return true;
        }

        private bool EnsureRightVersionStarted()
        {
            return rightVersionState != QuestState.NotStarted || StartRightVersion();
        }

        private bool ResolveElsa(bool protectedElsa)
        {
            if (!EnsureRightVersionStarted())
            {
                return false;
            }

            if (rightVersionState != QuestState.Active)
            {
                Debug.Log($"Cannot resolve Elsa: quest is {rightVersionState}.", this);
                return false;
            }

            rightVersionStage = RightVersionStage.FindMedallion;
            DecisionFlagService.Instance?.SetFlag(protectedElsa ? "ElsaProtected" : "ElsaBetrayed");
            if (!protectedElsa)
            {
                DecisionFlagService.Instance?.SetFlag("MayorSupported");
            }

            NotifyQuestChanged("Elsa decision recorded", RightVersionQuestId, rightVersionStage.ToString());
            return true;
        }

        private bool RecordMedallionFound()
        {
            if (!EnsureRightVersionStarted())
            {
                return false;
            }

            if (rightVersionState != QuestState.Active)
            {
                Debug.Log($"Cannot record medallion: quest is {rightVersionState}.", this);
                return false;
            }

            rightVersionStage = RightVersionStage.OpenTowerRoute;
            DecisionFlagService.Instance?.SetFlag("MedallionFound");
            NotifyQuestChanged("Medallion found", RightVersionQuestId, rightVersionStage.ToString());
            return true;
        }

        private bool OpenTowerRoute()
        {
            if (!EnsureRightVersionStarted())
            {
                return false;
            }

            if (rightVersionState == QuestState.Active)
            {
                rightVersionState = QuestState.Completed;
                rightVersionStage = RightVersionStage.Completed;
            }

            DecisionFlagService.Instance?.SetFlag("TowerRouteOpened");
            StartMirrorTruth();
            NotifyQuestChanged("Tower route opened", MirrorTruthQuestId, mirrorTruthStage.ToString());
            return true;
        }

        private bool StartMirrorTruth()
        {
            if (mirrorTruthState != QuestState.NotStarted)
            {
                return false;
            }

            mirrorTruthState = QuestState.Active;
            mirrorTruthStage = MirrorTruthStage.ReadOrtenDiary;
            DecisionFlagService.Instance?.SetFlag("mirrorTruthStarted");
            NotifyQuestChanged("Quest started", MirrorTruthQuestId, mirrorTruthStage.ToString());
            return true;
        }

        private bool EnsureMirrorTruthStarted()
        {
            if (mirrorTruthState != QuestState.NotStarted)
            {
                return true;
            }

            if (rightVersionState == QuestState.NotStarted)
            {
                StartRightVersion();
            }

            rightVersionState = QuestState.Completed;
            rightVersionStage = RightVersionStage.Completed;
            return StartMirrorTruth();
        }

        private bool RecordOrtenDiary()
        {
            if (!EnsureMirrorTruthStarted())
            {
                return false;
            }

            if (mirrorTruthState != QuestState.Active)
            {
                Debug.Log($"Cannot record Orten diary: quest is {mirrorTruthState}.", this);
                return false;
            }

            mirrorTruthStage = MirrorTruthStage.ConfrontOrten;
            DecisionFlagService.Instance?.SetFlag("OrtenDiaryFound");
            NotifyQuestChanged("Orten diary found", MirrorTruthQuestId, mirrorTruthStage.ToString());
            return true;
        }

        private bool ConfrontOrten()
        {
            if (!EnsureMirrorTruthStarted())
            {
                return false;
            }

            if (mirrorTruthState != QuestState.Active)
            {
                Debug.Log($"Cannot confront Orten: quest is {mirrorTruthState}.", this);
                return false;
            }

            mirrorTruthStage = MirrorTruthStage.ChooseEnding;
            DecisionFlagService.Instance?.SetFlag("OrtenConfronted");
            NotifyQuestChanged("Orten confronted", MirrorTruthQuestId, mirrorTruthStage.ToString());
            return true;
        }

        private bool DestroyMirrorShards()
        {
            if (!EnsureMirrorTruthStarted())
            {
                return false;
            }

            if (mirrorTruthState != QuestState.Active)
            {
                Debug.Log($"Cannot destroy mirror shards: quest is {mirrorTruthState}.", this);
                return false;
            }

            mirrorTruthStage = MirrorTruthStage.ChooseEnding;
            DecisionFlagService.Instance?.SetFlag("MirrorShardsDestroyed");
            NotifyQuestChanged("Mirror shards destroyed", MirrorTruthQuestId, mirrorTruthStage.ToString());
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

        private bool StartSmithDebt()
        {
            if (smithDebtState != QuestState.NotStarted)
            {
                Debug.Log($"Quest already started: {SmithDebtQuestId}", this);
                return false;
            }

            smithDebtState = QuestState.Active;
            smithDebtStage = SmithDebtStage.FindOldCampBlade;
            DecisionFlagService.Instance?.SetFlag("smithDebtStarted");
            NotifyQuestChanged("Quest started", SmithDebtQuestId, smithDebtStage.ToString());
            return true;
        }

        private bool FindOldCampBlade()
        {
            if (smithDebtState != QuestState.Active)
            {
                Debug.Log($"Cannot find old camp blade: quest is {smithDebtState}.", this);
                return false;
            }

            if (smithDebtStage != SmithDebtStage.FindOldCampBlade)
            {
                Debug.Log($"Cannot find old camp blade: current stage is {smithDebtStage}.", this);
                return false;
            }

            smithDebtStage = SmithDebtStage.ReturnToSmith;
            InventoryService.Instance?.AddItem("Old Camp Blade");
            DecisionFlagService.Instance?.SetFlag("oldCampBladeFound");
            NotifyQuestChanged("Old camp blade found", SmithDebtQuestId, smithDebtStage.ToString());
            return true;
        }

        private bool ReturnAndCompleteSmithDebt()
        {
            if (smithDebtState != QuestState.Active || smithDebtStage != SmithDebtStage.ReturnToSmith)
            {
                Debug.Log($"Cannot return {SmithDebtQuestId}: current stage is {smithDebtStage}.", this);
                return false;
            }

            smithDebtStage = SmithDebtStage.ReceiveReward;
            NotifyQuestChanged("Quest advanced", SmithDebtQuestId, smithDebtStage.ToString());
            return CompleteSmithDebt();
        }

        private bool CompleteSmithDebt()
        {
            if (smithDebtState != QuestState.Active || smithDebtStage != SmithDebtStage.ReceiveReward)
            {
                Debug.Log($"Cannot complete {SmithDebtQuestId}: current stage is {smithDebtStage}.", this);
                return false;
            }

            smithDebtState = QuestState.Completed;
            smithDebtStage = SmithDebtStage.Completed;
            PlayerRewardService.Instance?.GrantSmithDebtReward();
            NotifyQuestChanged("Quest completed", SmithDebtQuestId, smithDebtStage.ToString());
            return true;
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

        private string GetRightVersionObjective()
        {
            switch (rightVersionStage)
            {
                case RightVersionStage.FindElsa:
                    return "Find Elsa in the Black Swamp and hear the first version.";
                case RightVersionStage.DecideElsa:
                    return "Decide whether Elsa is witness, suspect, or bait.";
                case RightVersionStage.FindMedallion:
                    return "Find the girl's medallion near the drowned reeds.";
                case RightVersionStage.OpenTowerRoute:
                    return "Use Elsa's clue or the reed charm mark to open the tower route.";
                case RightVersionStage.Completed:
                    return "Right Version completed. The tower route is open.";
                default:
                    return "Follow the version the village tried to bury.";
            }
        }

        private string GetMirrorTruthObjective()
        {
            switch (mirrorTruthStage)
            {
                case MirrorTruthStage.EnterTower:
                    return "Enter the ruined tower above the swamp.";
                case MirrorTruthStage.ReadOrtenDiary:
                    return "Read Orten's diary in the tower ruins.";
                case MirrorTruthStage.ConfrontOrten:
                    return "Confront Orten in the mirror hall.";
                case MirrorTruthStage.ChooseEnding:
                    return "Return to the Ash Road altars and choose Truth, Lie, or Sacrifice.";
                case MirrorTruthStage.Completed:
                    return "Mirror of Truth completed.";
                default:
                    return "Follow the mirror's last memory.";
            }
        }

        private string GetSmithDebtObjective()
        {
            switch (smithDebtStage)
            {
                case SmithDebtStage.FindOldCampBlade:
                    return "Find the old camp blade in the Old Forest.";
                case SmithDebtStage.ReturnToSmith:
                    return "Return the old camp blade to Boris in Vereskovy Brod.";
                case SmithDebtStage.ReceiveReward:
                    return "Receive the Improved Steel Sword.";
                case SmithDebtStage.Completed:
                    return "Smith's Debt completed.";
                default:
                    return "Speak with Boris the smith.";
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
                rightVersionState = rightVersionState.ToString(),
                rightVersionStage = rightVersionStage.ToString(),
                mirrorTruthState = mirrorTruthState.ToString(),
                mirrorTruthStage = mirrorTruthStage.ToString(),
                missingHunterState = missingHunterState.ToString(),
                missingHunterStage = missingHunterStage.ToString(),
                missingHunterClueCount = missingHunterClueCount,
                smithDebtState = smithDebtState.ToString(),
                smithDebtStage = smithDebtStage.ToString()
            };
        }

        public void RestoreSnapshot(QuestSnapshot snapshot)
        {
            if (snapshot == null)
            {
                swampContractState = QuestState.NotStarted;
                swampContractStage = SwampContractStage.TalkToElder;
                swampTraceCount = 0;
                rightVersionState = QuestState.NotStarted;
                rightVersionStage = RightVersionStage.FindElsa;
                mirrorTruthState = QuestState.NotStarted;
                mirrorTruthStage = MirrorTruthStage.EnterTower;
                missingHunterState = QuestState.NotStarted;
                missingHunterStage = MissingHunterStage.FindClues;
                missingHunterClueCount = 0;
                smithDebtState = QuestState.NotStarted;
                smithDebtStage = SmithDebtStage.FindOldCampBlade;
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
            if (!Enum.TryParse(snapshot.rightVersionState, out rightVersionState))
            {
                rightVersionState = QuestState.NotStarted;
            }

            if (!Enum.TryParse(snapshot.rightVersionStage, out rightVersionStage))
            {
                rightVersionStage = RightVersionStage.FindElsa;
            }

            if (!Enum.TryParse(snapshot.mirrorTruthState, out mirrorTruthState))
            {
                mirrorTruthState = QuestState.NotStarted;
            }

            if (!Enum.TryParse(snapshot.mirrorTruthStage, out mirrorTruthStage))
            {
                mirrorTruthStage = MirrorTruthStage.EnterTower;
            }

            if (!Enum.TryParse(snapshot.missingHunterState, out missingHunterState))
            {
                missingHunterState = QuestState.NotStarted;
            }

            if (!Enum.TryParse(snapshot.missingHunterStage, out missingHunterStage))
            {
                missingHunterStage = MissingHunterStage.FindClues;
            }

            missingHunterClueCount = Mathf.Clamp(snapshot.missingHunterClueCount, 0, RequiredMissingHunterClueCount);
            if (!Enum.TryParse(snapshot.smithDebtState, out smithDebtState))
            {
                smithDebtState = QuestState.NotStarted;
            }

            if (!Enum.TryParse(snapshot.smithDebtStage, out smithDebtStage))
            {
                smithDebtStage = SmithDebtStage.FindOldCampBlade;
            }

            NotifyQuestChanged("Quest restored");
        }
    }

    [Serializable]
    public sealed class QuestSnapshot
    {
        public string swampContractState;
        public string swampContractStage;
        public int swampTraceCount;
        public string rightVersionState;
        public string rightVersionStage;
        public string mirrorTruthState;
        public string mirrorTruthStage;
        public string missingHunterState;
        public string missingHunterStage;
        public int missingHunterClueCount;
        public string smithDebtState;
        public string smithDebtStage;
    }
}
