using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.Quest;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Combat
{
    [RequireComponent(typeof(Health))]
    public sealed class EnemyAI : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string enemyName = "Enemy";
        [SerializeField] private bool onlyActiveDuringDrownerStage = true;
        [SerializeField] private string deathFlagToSet = "killedFirstDrowner";
        [SerializeField] private string questActionOnDeath = QuestService.ActionFirstDrownerKilled;

        [Header("Movement")]
        [SerializeField] private float aggroRange = 7f;
        [SerializeField] private float attackRange = 1.45f;
        [SerializeField] private float moveSpeed = 2.15f;
        [SerializeField] private float turnSpeed = 10f;

        [Header("Attack")]
        [SerializeField] private float damage = 12f;
        [SerializeField] private float attackCooldown = 1.4f;

        private Health health;
        private Health targetHealth;
        private Transform target;
        private Renderer[] renderers;
        private Collider[] colliders;
        private bool lastCombatActiveState = true;
        private float nextAttackTime;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Died += HandleDeath;
            renderers = GetComponentsInChildren<Renderer>();
            colliders = GetComponentsInChildren<Collider>();
        }

        private void Start()
        {
            FindTarget();
            ApplyCombatActiveState(IsCombatActive());
        }

        public void Configure(string newEnemyName, bool newOnlyActiveDuringDrownerStage, string newDeathFlagToSet, string newQuestActionOnDeath)
        {
            enemyName = newEnemyName;
            onlyActiveDuringDrownerStage = newOnlyActiveDuringDrownerStage;
            deathFlagToSet = newDeathFlagToSet;
            questActionOnDeath = newQuestActionOnDeath;
        }

        private void Update()
        {
            var combatActive = IsCombatActive();
            if (combatActive != lastCombatActiveState)
            {
                ApplyCombatActiveState(combatActive);
            }

            if (!combatActive || health.IsDead)
            {
                return;
            }

            if (target == null || targetHealth == null)
            {
                FindTarget();
                return;
            }

            if (targetHealth.IsDead)
            {
                return;
            }

            var offset = target.position - transform.position;
            offset.y = 0f;
            var distance = offset.magnitude;

            if (distance > aggroRange)
            {
                return;
            }

            if (offset.sqrMagnitude > 0.01f)
            {
                var rotation = Quaternion.LookRotation(offset.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
            }

            if (distance > attackRange)
            {
                transform.position += offset.normalized * moveSpeed * Time.deltaTime;
                return;
            }

            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                targetHealth.TakeDamage(damage, gameObject);
                InteractionPromptUI.Instance?.ShowMessage($"{enemyName} hits Reynard: {damage:0} damage.");
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
            }
        }

        private void FindTarget()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            target = player.transform;
            targetHealth = player.GetComponent<Health>();
        }

        private bool IsCombatActive()
        {
            if (health.IsDead)
            {
                return false;
            }

            if (!onlyActiveDuringDrownerStage)
            {
                return true;
            }

            var quest = QuestService.Instance;
            return quest != null
                && quest.SwampContractState == QuestState.Active
                && quest.CurrentSwampContractStage == SwampContractStage.KillDrowner;
        }

        private void ApplyCombatActiveState(bool isActive)
        {
            lastCombatActiveState = isActive;

            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = isActive;
                }
            }

            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = isActive;
                }
            }
        }

        private void HandleDeath(Health deadHealth, GameObject source)
        {
            ApplyCombatActiveState(false);

            if (!string.IsNullOrWhiteSpace(deathFlagToSet))
            {
                DecisionFlagService.Instance?.SetFlag(deathFlagToSet);
            }

            if (!string.IsNullOrWhiteSpace(questActionOnDeath))
            {
                QuestService.Instance?.RunAction(questActionOnDeath);
            }

            InteractionPromptUI.Instance?.ShowMessage($"{enemyName} is dead. Return to Elder Voytsekh.");
        }
    }
}
