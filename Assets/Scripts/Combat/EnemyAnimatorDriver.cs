using UnityEngine;

namespace WitcherRightVersion.Combat
{
    /// <summary>
    /// Drives an enemy model's Mecanim Animator: Speed from measured movement,
    /// Attack trigger on AI attack, Dead on death. The Animator lives on the model
    /// child; root motion is off so the AI/CharacterController owns position.
    /// </summary>
    [RequireComponent(typeof(EnemyAI))]
    [RequireComponent(typeof(Health))]
    public sealed class EnemyAnimatorDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float speedSmooth = 10f;

        private EnemyAI ai;
        private Health health;
        private Vector3 lastPosition;
        private float smoothedSpeed;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int DeadHash = Animator.StringToHash("Dead");

        private void Awake()
        {
            ai = GetComponent<EnemyAI>();
            health = GetComponent<Health>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
            }

            lastPosition = transform.position;
        }

        private void OnEnable()
        {
            if (ai != null)
            {
                ai.AttackStarted += HandleAttack;
            }

            if (health != null)
            {
                health.Died += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (ai != null)
            {
                ai.AttackStarted -= HandleAttack;
            }

            if (health != null)
            {
                health.Died -= HandleDied;
            }
        }

        private void Update()
        {
            if (animator == null)
            {
                return;
            }

            var delta = transform.position - lastPosition;
            lastPosition = transform.position;
            delta.y = 0f;
            var measured = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            smoothedSpeed = Mathf.MoveTowards(smoothedSpeed, measured, speedSmooth * Time.deltaTime);
            animator.SetFloat(SpeedHash, smoothedSpeed);
        }

        private void HandleAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger(AttackHash);
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
