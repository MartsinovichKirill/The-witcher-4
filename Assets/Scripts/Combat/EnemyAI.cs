using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.Quest;
using WitcherRightVersion.UI;
using System;

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
        [SerializeField] private string requiredFlagToBecomeActive;
        [SerializeField] private string deathMessage = "Enemy is dead.";

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
        private float stunEndTime;

        public event Action AttackStarted;
        public event Action<float> Stunned;

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

        public void Configure(string newEnemyName, bool newOnlyActiveDuringDrownerStage, string newDeathFlagToSet, string newQuestActionOnDeath, string newRequiredFlagToBecomeActive = "", string newDeathMessage = "")
        {
            enemyName = newEnemyName;
            onlyActiveDuringDrownerStage = newOnlyActiveDuringDrownerStage;
            deathFlagToSet = newDeathFlagToSet;
            questActionOnDeath = newQuestActionOnDeath;
            requiredFlagToBecomeActive = newRequiredFlagToBecomeActive;
            deathMessage = string.IsNullOrWhiteSpace(newDeathMessage)
                ? $"{enemyName} is dead."
                : newDeathMessage;
        }

        public void ConfigureCombat(float newAggroRange, float newAttackRange, float newMoveSpeed, float newDamage, float newAttackCooldown)
        {
            aggroRange = Mathf.Max(0.5f, newAggroRange);
            attackRange = Mathf.Max(0.35f, newAttackRange);
            moveSpeed = Mathf.Max(0.1f, newMoveSpeed);
            damage = Mathf.Max(0f, newDamage);
            attackCooldown = Mathf.Max(0.2f, newAttackCooldown);
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

            if (Time.time < stunEndTime)
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
                AttackStarted?.Invoke();
                var targetCombat = target.GetComponent<CombatController>();
                if (targetCombat != null)
                {
                    targetCombat.ReceiveEnemyAttack(damage, gameObject);
                }
                else
                {
                    targetHealth.TakeDamage(damage, gameObject);
                    InteractionPromptUI.Instance?.ShowMessage($"{enemyName} hits target: {damage:0} damage.");
                }
            }
        }

        public void ApplyAard(Vector3 direction, float knockbackDistance, float stunDuration)
        {
            if (health.IsDead)
            {
                return;
            }

            var flatDirection = direction;
            flatDirection.y = 0f;
            if (flatDirection.sqrMagnitude <= 0.01f)
            {
                flatDirection = -transform.forward;
            }

            transform.position += flatDirection.normalized * Mathf.Max(0f, knockbackDistance);
            var duration = Mathf.Max(0f, stunDuration);
            stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);
            Stunned?.Invoke(duration);
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
                return string.IsNullOrWhiteSpace(requiredFlagToBecomeActive)
                    || (DecisionFlagService.Instance != null && DecisionFlagService.Instance.HasFlag(requiredFlagToBecomeActive));
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
            lastCombatActiveState = false;

            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(deathFlagToSet))
            {
                DecisionFlagService.Instance?.SetFlag(deathFlagToSet);
            }

            if (!string.IsNullOrWhiteSpace(questActionOnDeath))
            {
                QuestService.Instance?.RunAction(questActionOnDeath);
            }

            InteractionPromptUI.Instance?.ShowMessage(deathMessage);
        }
    }
}
