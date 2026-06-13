using UnityEngine;
using WitcherRightVersion.Combat;

namespace WitcherRightVersion.Player
{
    /// <summary>
    /// Drives the Knight model's Mecanim Animator from gameplay state: Speed for
    /// idle/walk/run, Attack/Dodge triggers from combat, Dead on death. The Animator
    /// lives on the model child; root motion is off so PlayerController owns position.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class CharacterAnimatorDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float walkSpeedValue = 3.5f;
        [SerializeField] private float runSpeedValue = 6.25f;
        [SerializeField] private float speedSmooth = 12f;

        private PlayerController movement;
        private CombatController combat;
        private Health health;
        private float smoothedSpeed;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int DodgeHash = Animator.StringToHash("Dodge");
        private static readonly int DeadHash = Animator.StringToHash("Dead");

        private void Awake()
        {
            movement = GetComponent<PlayerController>();
            combat = GetComponent<CombatController>();
            health = GetComponent<Health>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
        }

        private void OnEnable()
        {
            if (combat != null)
            {
                combat.LightAttackStarted += HandleAttack;
                combat.HeavyAttackStarted += HandleAttack;
                combat.DodgeStarted += HandleDodge;
            }

            if (health != null)
            {
                health.Died += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (combat != null)
            {
                combat.LightAttackStarted -= HandleAttack;
                combat.HeavyAttackStarted -= HandleAttack;
                combat.DodgeStarted -= HandleDodge;
            }

            if (health != null)
            {
                health.Died -= HandleDied;
            }
        }

        private void Update()
        {
            if (animator == null || movement == null)
            {
                return;
            }

            var target = movement.IsMoving ? (movement.IsRunning ? runSpeedValue : walkSpeedValue) : 0f;
            smoothedSpeed = Mathf.MoveTowards(smoothedSpeed, target, speedSmooth * Time.deltaTime);
            animator.SetFloat(SpeedHash, smoothedSpeed);
        }

        private void HandleAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger(AttackHash);
            }
        }

        private void HandleDodge()
        {
            if (animator != null)
            {
                animator.SetTrigger(DodgeHash);
            }
        }

        private void HandleDied(Health deadHealth, GameObject source)
        {
            if (animator != null)
            {
                animator.SetBool(DeadHash, true);
            }
        }
    }
}
