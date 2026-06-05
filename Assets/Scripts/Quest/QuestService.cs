using System;
using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Localization;

namespace WitcherRightVersion.Quest
{
    public sealed class QuestService : MonoBehaviour
    {
        private const int RequiredSwampTraceCount = 3;
        private const int RequiredMissingHunterClueCount = 2;
        private const int RequiredDrownerNestKillCount = 3;

        public const string SwampContractQuestId = "contract_swamp_beast";
        public const string RightVersionQuestId = "right_version";
        public const string MirrorTruthQuestId = "mirror_truth";
        public const string MissingHunterQuestId = "missing_hunter";
        public const string SmithDebtQuestId = "smith_debt";
        public const string VoiceWellQuestId = "voice_well";
        public const string DrownerNestQuestId = "drowner_nest";
        public const string ExileQuestId = "exile";
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
        public const string ActionStartVoiceWell = "start_voice_well";
        public const string ActionGhostMemoryHeard = "ghost_memory_heard";
        public const string ActionStartDrownerNest = "start_drowner_nest";
        public const string ActionDrownerNestEnemyKilled = "drowner_nest_enemy_killed";
        public const string ActionDrownerNestRewardReceived = "drowner_nest_reward_received";
        public const string ActionStartExile = "start_exile";

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
        private QuestState voiceWellState = QuestState.NotStarted;
        private VoiceWellStage voiceWellStage = VoiceWellStage.ListenAtWell;
        private QuestState drownerNestState = QuestState.NotStarted;
        private DrownerNestStage drownerNestStage = DrownerNestStage.AcceptNotice;
        private int drownerNestKillCount;
        private QuestState exileState = QuestState.NotStarted;
        private ExileStage exileStage = ExileStage.FindElsa;

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
        public QuestState VoiceWellState => voiceWellState;
        public VoiceWellStage CurrentVoiceWellStage => voiceWellStage;
        public QuestState DrownerNestState => drownerNestState;
        public DrownerNestStage CurrentDrownerNestStage => drownerNestStage;
        public int DrownerNestKillCount => drownerNestKillCount;
        public int DrownerNestKillTarget => RequiredDrownerNestKillCount;
        public QuestState ExileState => exileState;
        public ExileStage CurrentExileStage => exileStage;
        public bool HasActiveQuest => swampContractState == QuestState.Active
            || rightVersionState == QuestState.Active
            || mirrorTruthState == QuestState.Active
            || missingHunterState == QuestState.Active
            || smithDebtState == QuestState.Active
            || voiceWellState == QuestState.Active
            || drownerNestState == QuestState.Active
            || exileState == QuestState.Active
            || swampContractState == QuestState.Completed
            || rightVersionState == QuestState.Completed
            || mirrorTruthState == QuestState.Completed
            || missingHunterState == QuestState.Completed
            || smithDebtState == QuestState.Completed
            || voiceWellState == QuestState.Completed
            || drownerNestState == QuestState.Completed
            || exileState == QuestState.Completed;

        public string ActiveQuestTitle
        {
            get
            {
                if (mirrorTruthState == QuestState.Active || mirrorTruthState == QuestState.Completed)
                {
                    return "Mirror of Truth";
                }

                if (voiceWellState == QuestState.Active || voiceWellState == QuestState.Completed)
                {
                    return "Voice from the Well";
                }

                if (exileState == QuestState.Active || exileState == QuestState.Completed)
                {
                    return "Exile";
                }

                if (rightVersionState == QuestState.Active || rightVersionState == QuestState.Completed)
                {
                    return "Right Version";
                }

                if (swampContractState == QuestState.Active)
                {
                    return "Contract: Beast from the Swamp";
                }

                if (drownerNestState == QuestState.Active)
                {
                    return "Drowner Nest";
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

                if (drownerNestState == QuestState.Completed)
                {
                    return "Drowner Nest";
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

                if (voiceWellState == QuestState.Active || voiceWellState == QuestState.Completed)
                {
                    return GetVoiceWellObjective();
                }

                if (exileState == QuestState.Active || exileState == QuestState.Completed)
                {
                    return GetExileObjective();
                }

                if (rightVersionState == QuestState.Active || rightVersionState == QuestState.Completed)
                {
                    return GetRightVersionObjective();
                }

                if (swampContractState == QuestState.Active)
                {
                    return GetSwampContractObjective();
                }

                if (drownerNestState == QuestState.Active || drownerNestState == QuestState.Completed)
                {
                    return GetDrownerNestObjective();
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
                case ActionStartVoiceWell:
                    return StartVoiceWell();
                case ActionGhostMemoryHeard:
                    return RecordGhostMemoryHeard();
                case ActionStartDrownerNest:
                    return StartDrownerNest();
                case ActionDrownerNestEnemyKilled:
                    return RecordDrownerNestKill();
                case ActionDrownerNestRewardReceived:
                    return CompleteDrownerNest();
                case ActionStartExile:
                    return StartExile();
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
            if (exileState == QuestState.Completed)
            {
                Debug.Log($"Cannot resolve Elsa: {ExileQuestId} is already completed.", this);
                return false;
            }

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

            CompleteExile(protectedElsa);
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
            InventoryService.Instance?.AddItem("Girl's Medallion");
            AdvanceVoiceWellAfterMedallion();
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

        private bool StartVoiceWell()
        {
            if (voiceWellState != QuestState.NotStarted)
            {
                Debug.Log($"Quest already started: {VoiceWellQuestId}", this);
                return false;
            }

            voiceWellState = QuestState.Active;
            voiceWellStage = DecisionFlagService.Instance != null && DecisionFlagService.Instance.HasFlag("MedallionFound")
                ? VoiceWellStage.HearGhostMemory
                : VoiceWellStage.FindMedallion;
            DecisionFlagService.Instance?.SetFlag("voiceWellStarted");
            NotifyQuestChanged("Quest started", VoiceWellQuestId, voiceWellStage.ToString());
            return true;
        }

        private void AdvanceVoiceWellAfterMedallion()
        {
            if (voiceWellState == QuestState.Completed)
            {
                return;
            }

            if (voiceWellState == QuestState.NotStarted)
            {
                voiceWellState = QuestState.Active;
                DecisionFlagService.Instance?.SetFlag("voiceWellStarted");
            }

            voiceWellStage = VoiceWellStage.HearGhostMemory;
            NotifyQuestChanged("Medallion recovered", VoiceWellQuestId, voiceWellStage.ToString());
        }

        private bool RecordGhostMemoryHeard()
        {
            if (voiceWellState != QuestState.Active)
            {
                Debug.Log($"Cannot record ghost memory: quest is {voiceWellState}.", this);
                return false;
            }

            if (voiceWellStage != VoiceWellStage.HearGhostMemory)
            {
                Debug.Log($"Cannot record ghost memory: current stage is {voiceWellStage}.", this);
                return false;
            }

            voiceWellState = QuestState.Completed;
            voiceWellStage = VoiceWellStage.Completed;
            DecisionFlagService.Instance?.SetFlag("GhostMemoryHeard");
            PlayerRewardService.Instance?.GrantVoiceWellReward();
            NotifyQuestChanged("Quest completed", VoiceWellQuestId, voiceWellStage.ToString());
            return true;
        }

        private bool StartDrownerNest()
        {
            if (drownerNestState != QuestState.NotStarted)
            {
                Debug.Log($"Quest already started: {DrownerNestQuestId}", this);
                return false;
            }

            drownerNestState = QuestState.Active;
            drownerNestStage = DrownerNestStage.ClearNest;
            drownerNestKillCount = 0;
            DecisionFlagService.Instance?.SetFlag("drownerNestStarted");
            NotifyQuestChanged("Quest started", DrownerNestQuestId, drownerNestStage.ToString());
            return true;
        }

        private bool RecordDrownerNestKill()
        {
            if (drownerNestState != QuestState.Active)
            {
                Debug.Log($"Cannot record drowner nest kill: quest is {drownerNestState}.", this);
                return false;
            }

            if (drownerNestStage != DrownerNestStage.ClearNest)
            {
                Debug.Log($"Cannot record drowner nest kill: current stage is {drownerNestStage}.", this);
                return false;
            }

            drownerNestKillCount = Mathf.Clamp(drownerNestKillCount + 1, 0, RequiredDrownerNestKillCount);
            if (drownerNestKillCount >= RequiredDrownerNestKillCount)
            {
                drownerNestStage = DrownerNestStage.ReturnForReward;
                DecisionFlagService.Instance?.SetFlag("DrownerNestCleared");
                NotifyQuestChanged("Nest cleared", DrownerNestQuestId, drownerNestStage.ToString());
                return true;
            }

            NotifyQuestChanged("Drowner killed", DrownerNestQuestId, drownerNestStage.ToString());
            return true;
        }

        private bool CompleteDrownerNest()
        {
            if (drownerNestState != QuestState.Active || drownerNestStage != DrownerNestStage.ReturnForReward)
            {
                Debug.Log($"Cannot complete {DrownerNestQuestId}: current stage is {drownerNestStage}.", this);
                return false;
            }

            drownerNestState = QuestState.Completed;
            drownerNestStage = DrownerNestStage.Completed;
            PlayerRewardService.Instance?.GrantDrownerNestReward();
            NotifyQuestChanged("Quest completed", DrownerNestQuestId, drownerNestStage.ToString());
            return true;
        }

        private bool StartExile()
        {
            EnsureRightVersionStarted();

            if (exileState == QuestState.Completed)
            {
                Debug.Log($"Quest already completed: {ExileQuestId}", this);
                return false;
            }

            if (exileState == QuestState.NotStarted)
            {
                exileState = QuestState.Active;
                exileStage = ExileStage.DecideElsa;
                DecisionFlagService.Instance?.SetFlag("exileQuestStarted");
                NotifyQuestChanged("Quest started", ExileQuestId, exileStage.ToString());
                return true;
            }

            exileStage = ExileStage.DecideElsa;
            NotifyQuestChanged("Quest advanced", ExileQuestId, exileStage.ToString());
            return true;
        }

        private void CompleteExile(bool protectedElsa)
        {
            if (exileState == QuestState.NotStarted)
            {
                exileState = QuestState.Active;
            }

            exileStage = ExileStage.Completed;
            exileState = QuestState.Completed;
            if (protectedElsa)
            {
                PlayerRewardService.Instance?.GrantExileProtectedReward();
            }
            else
            {
                PlayerRewardService.Instance?.GrantExileBetrayedReward();
            }

            NotifyQuestChanged("Quest completed", ExileQuestId, exileStage.ToString());
        }

        private string GetSwampContractObjective()
        {
            switch (swampContractStage)
            {
                case SwampContractStage.SpeakWithMarta:
                    return GameLocalization.Select("Speak with Marta about swamp poison.", "Поговорить с Мартой о болотном яде.");
                case SwampContractStage.FindSwampTraces:
                    return GameLocalization.Select($"Inspect swamp traces south of the village ({swampTraceCount}/{RequiredSwampTraceCount}).", $"Осмотреть следы на болоте к югу от деревни ({swampTraceCount}/{RequiredSwampTraceCount}).");
                case SwampContractStage.KillDrowner:
                    return GameLocalization.Select("Kill the drowner near the swamp road.", "Убить утопца у болотной дороги.");
                case SwampContractStage.ReturnToElder:
                    return GameLocalization.Select("Return to Elder Voytsekh with proof.", "Вернуться к старосте Войцеху с доказательством.");
                case SwampContractStage.ChooseResponse:
                    return GameLocalization.Select("Choose what to tell the elder.", "Выбрать, что сказать старосте.");
                case SwampContractStage.ReceiveReward:
                    return GameLocalization.Select("Receive 50 XP, 20 coins, and the Antitoxin recipe.", "Получить 50 опыта, 20 монет и рецепт противоядия.");
                case SwampContractStage.Completed:
                    return GameLocalization.Select("Contract completed.", "Контракт завершён.");
                default:
                    return GameLocalization.Select("Talk to Elder Voytsekh.", "Поговорить со старостой Войцехом.");
            }
        }

        private string GetMissingHunterObjective()
        {
            switch (missingHunterStage)
            {
                case MissingHunterStage.FindClues:
                    return GameLocalization.Select($"Search the Old Forest for hunter signs ({missingHunterClueCount}/{RequiredMissingHunterClueCount}).", $"Найти следы охотника в Старом Лесу ({missingHunterClueCount}/{RequiredMissingHunterClueCount}).");
                case MissingHunterStage.ReturnToCamp:
                    return GameLocalization.Select("Return to the hunter camp and decide what the signs mean.", "Вернуться в лагерь охотника и понять, что значат следы.");
                case MissingHunterStage.ReceiveReward:
                    return GameLocalization.Select("Take the hunter's emergency pouch: 25 XP and 10 coins.", "Забрать тайный кошель охотника: 25 опыта и 10 монет.");
                case MissingHunterStage.Completed:
                    return GameLocalization.Select("Missing Hunter completed.", "Квест «Пропавший охотник» завершён.");
                default:
                    return GameLocalization.Select("Search the Old Forest.", "Обыскать Старый Лес.");
            }
        }

        private string GetRightVersionObjective()
        {
            switch (rightVersionStage)
            {
                case RightVersionStage.FindElsa:
                    return GameLocalization.Select("Find Elsa in the Black Swamp and hear the first version.", "Найти Эльзу в Чёрном Болоте и услышать первую версию.");
                case RightVersionStage.DecideElsa:
                    return GameLocalization.Select("Decide whether Elsa is witness, suspect, or bait.", "Решить, кто Эльза: свидетель, подозреваемая или приманка.");
                case RightVersionStage.FindMedallion:
                    return GameLocalization.Select("Find the girl's medallion near the drowned reeds.", "Найти медальон девушки у затопленных камышей.");
                case RightVersionStage.OpenTowerRoute:
                    return GameLocalization.Select("Use Elsa's clue or the reed charm mark to open the tower route.", "Использовать подсказку Эльзы или знак камышового оберега, чтобы открыть путь к башне.");
                case RightVersionStage.Completed:
                    return GameLocalization.Select("Right Version completed. The tower route is open.", "«Правильная версия» завершена. Путь к башне открыт.");
                default:
                    return GameLocalization.Select("Follow the version the village tried to bury.", "Идти за версией, которую деревня пыталась похоронить.");
            }
        }

        private string GetMirrorTruthObjective()
        {
            switch (mirrorTruthStage)
            {
                case MirrorTruthStage.EnterTower:
                    return GameLocalization.Select("Enter the ruined tower above the swamp.", "Войти в разрушенную башню над болотом.");
                case MirrorTruthStage.ReadOrtenDiary:
                    return GameLocalization.Select("Read Orten's diary in the tower ruins.", "Прочитать дневник Ортена в руинах башни.");
                case MirrorTruthStage.ConfrontOrten:
                    return GameLocalization.Select("Confront Orten in the mirror hall.", "Противостоять Ортену в зеркальном зале.");
                case MirrorTruthStage.ChooseEnding:
                    return GameLocalization.Select("Return to the Ash Road altars and choose Truth, Lie, or Sacrifice.", "Вернуться к алтарям на Пепельном тракте и выбрать Правду, Ложь или Жертву.");
                case MirrorTruthStage.Completed:
                    return GameLocalization.Select("Mirror of Truth completed.", "«Зеркало Правды» завершено.");
                default:
                    return GameLocalization.Select("Follow the mirror's last memory.", "Следовать последнему воспоминанию зеркала.");
            }
        }

        private string GetSmithDebtObjective()
        {
            switch (smithDebtStage)
            {
                case SmithDebtStage.FindOldCampBlade:
                    return GameLocalization.Select("Find the old camp blade in the Old Forest.", "Найти старый лагерный клинок в Старом Лесу.");
                case SmithDebtStage.ReturnToSmith:
                    return GameLocalization.Select("Return the old camp blade to Boris in Vereskovy Brod.", "Вернуть старый клинок Борису в Вересковом Броде.");
                case SmithDebtStage.ReceiveReward:
                    return GameLocalization.Select("Receive the Improved Steel Sword.", "Получить улучшенный стальной меч.");
                case SmithDebtStage.Completed:
                    return GameLocalization.Select("Smith's Debt completed.", "«Долг кузнеца» завершён.");
                default:
                    return GameLocalization.Select("Speak with Boris the smith.", "Поговорить с кузнецом Борисом.");
            }
        }

        private string GetVoiceWellObjective()
        {
            switch (voiceWellStage)
            {
                case VoiceWellStage.ListenAtWell:
                    return GameLocalization.Select("Listen to the old village well.", "Прислушаться к старому деревенскому колодцу.");
                case VoiceWellStage.FindMedallion:
                    return GameLocalization.Select("Find the girl's medallion near the drowned reeds.", "Найти медальон девушки у затопленных камышей.");
                case VoiceWellStage.HearGhostMemory:
                    return GameLocalization.Select("Listen to the ghost memory in the tower ruins.", "Выслушать воспоминание призрака в руинах башни.");
                case VoiceWellStage.Completed:
                    return GameLocalization.Select("Voice from the Well completed.", "«Голос из колодца» завершён.");
                default:
                    return GameLocalization.Select("Follow the voice buried under the village.", "Следовать голосу, похороненному под деревней.");
            }
        }

        private string GetDrownerNestObjective()
        {
            switch (drownerNestStage)
            {
                case DrownerNestStage.AcceptNotice:
                    return GameLocalization.Select("Read the drowner nest notice in Vereskovy Brod.", "Прочитать объявление о логове утопцев в Вересковом Броде.");
                case DrownerNestStage.ClearNest:
                    return GameLocalization.Select($"Clear the drowner nest in the Black Swamp ({drownerNestKillCount}/{RequiredDrownerNestKillCount}).", $"Зачистить логово утопцев в Чёрном Болоте ({drownerNestKillCount}/{RequiredDrownerNestKillCount}).");
                case DrownerNestStage.ReturnForReward:
                    return GameLocalization.Select("Return to the notice board cache for the drowner nest reward.", "Вернуться к тайнику у доски объявлений за наградой за логово утопцев.");
                case DrownerNestStage.Completed:
                    return GameLocalization.Select("Drowner Nest completed. The swamp is safer.", "«Логово утопцев» завершено. Болото стало безопаснее.");
                default:
                    return GameLocalization.Select("Clear the drowner nest.", "Зачистить логово утопцев.");
            }
        }

        private string GetExileObjective()
        {
            switch (exileStage)
            {
                case ExileStage.FindElsa:
                    return GameLocalization.Select("Find Elsa Cherntravka in the Black Swamp.", "Найти Эльзу Чернотравку в Чёрном Болоте.");
                case ExileStage.HearHerVersion:
                    return GameLocalization.Select("Hear Elsa's version of the buried murder.", "Выслушать версию Эльзы о похороненном убийстве.");
                case ExileStage.DecideElsa:
                    return GameLocalization.Select("Decide whether to protect Elsa or hand her to Voytsekh.", "Решить, защитить Эльзу или выдать её Войцеху.");
                case ExileStage.Completed:
                    return GameLocalization.Select("Exile completed. Elsa's fate is now part of the final consequences.", "«Изгнанница» завершена. Судьба Эльзы теперь влияет на финал.");
                default:
                    return GameLocalization.Select("Decide Elsa's fate.", "Решить судьбу Эльзы.");
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
                smithDebtStage = smithDebtStage.ToString(),
                voiceWellState = voiceWellState.ToString(),
                voiceWellStage = voiceWellStage.ToString(),
                drownerNestState = drownerNestState.ToString(),
                drownerNestStage = drownerNestStage.ToString(),
                drownerNestKillCount = drownerNestKillCount,
                exileState = exileState.ToString(),
                exileStage = exileStage.ToString()
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
                voiceWellState = QuestState.NotStarted;
                voiceWellStage = VoiceWellStage.ListenAtWell;
                drownerNestState = QuestState.NotStarted;
                drownerNestStage = DrownerNestStage.AcceptNotice;
                drownerNestKillCount = 0;
                exileState = QuestState.NotStarted;
                exileStage = ExileStage.FindElsa;
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

            if (!Enum.TryParse(snapshot.voiceWellState, out voiceWellState))
            {
                voiceWellState = QuestState.NotStarted;
            }

            if (!Enum.TryParse(snapshot.voiceWellStage, out voiceWellStage))
            {
                voiceWellStage = VoiceWellStage.ListenAtWell;
            }

            if (!Enum.TryParse(snapshot.drownerNestState, out drownerNestState))
            {
                drownerNestState = QuestState.NotStarted;
            }

            if (!Enum.TryParse(snapshot.drownerNestStage, out drownerNestStage))
            {
                drownerNestStage = DrownerNestStage.AcceptNotice;
            }

            drownerNestKillCount = Mathf.Clamp(snapshot.drownerNestKillCount, 0, RequiredDrownerNestKillCount);
            if (!Enum.TryParse(snapshot.exileState, out exileState))
            {
                exileState = QuestState.NotStarted;
            }

            if (!Enum.TryParse(snapshot.exileStage, out exileStage))
            {
                exileStage = ExileStage.FindElsa;
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
        public string voiceWellState;
        public string voiceWellStage;
        public string drownerNestState;
        public string drownerNestStage;
        public int drownerNestKillCount;
        public string exileState;
        public string exileStage;
    }
}
