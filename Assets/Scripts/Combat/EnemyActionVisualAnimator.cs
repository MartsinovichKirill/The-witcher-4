using UnityEngine;

namespace WitcherRightVersion.Combat
{
    [RequireComponent(typeof(EnemyAI))]
    [RequireComponent(typeof(Health))]
    public sealed class EnemyActionVisualAnimator : MonoBehaviour
    {
        private enum Pose
        {
            None,
            Attack,
            Hit,
            Stun,
            Dead
        }

        [SerializeField] private Transform visualRoot;
        [SerializeField] private float walkBobHeight = 0.035f;
        [SerializeField] private float walkBobSpeed = 8f;
        [SerializeField] private float moveLean = 5f;
        [SerializeField] private float attackDuration = 0.34f;
        [SerializeField] private float hitDuration = 0.22f;
        [SerializeField] private float stunDuration = 0.35f;
        [SerializeField] private float poseSmooth = 15f;
        [SerializeField] private bool quadruped;

        private EnemyAI ai;
        private Health health;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private Vector3 lastWorldPosition;
        private Pose currentPose;
        private float poseStartedAt;
        private float poseLength;

        private void Awake()
        {
            ai = GetComponent<EnemyAI>();
            health = GetComponent<Health>();
            ResolveVisualRoot();
            CaptureRestPose();
            lastWorldPosition = transform.position;
        }

        private void OnEnable()
        {
            ai.AttackStarted += HandleAttackStarted;
            ai.Stunned += HandleStunned;
            health.Damaged += HandleDamaged;
            health.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (ai != null)
            {
                ai.AttackStarted -= HandleAttackStarted;
                ai.Stunned -= HandleStunned;
            }

            if (health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Died -= HandleDied;
            }
        }

        private void LateUpdate()
        {
            if (visualRoot == null)
            {
                return;
            }

            var desiredPosition = restPosition;
            var desiredRotation = restRotation;
            var velocity = (transform.position - lastWorldPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastWorldPosition = transform.position;

            if (currentPose != Pose.Dead)
            {
                ApplyLocomotion(velocity, ref desiredPosition, ref desiredRotation);
            }

            ApplyPose(ref desiredPosition, ref desiredRotation);

            var blend = 1f - Mathf.Exp(-poseSmooth * Time.deltaTime);
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, desiredPosition, blend);
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, desiredRotation, blend);
        }

        public void Configure(Transform newVisualRoot, bool isQuadruped)
        {
            visualRoot = newVisualRoot;
            quadruped = isQuadruped;
            CaptureRestPose();
        }

        private void ResolveVisualRoot()
        {
            if (visualRoot != null)
            {
                return;
            }

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.GetComponentInChildren<Renderer>(true) != null)
                {
                    visualRoot = child;
                    return;
                }
            }
        }

        private void CaptureRestPose()
        {
            if (visualRoot == null)
            {
                return;
            }

            restPosition = visualRoot.localPosition;
            restRotation = visualRoot.localRotation;
        }

        private void ApplyLocomotion(Vector3 worldVelocity, ref Vector3 position, ref Quaternion rotation)
        {
            var flatVelocity = worldVelocity;
            flatVelocity.y = 0f;
            var speed = flatVelocity.magnitude;
            if (speed < 0.08f || currentPose != Pose.None)
            {
                return;
            }

            var cycle = Time.time * walkBobSpeed;
            var stride = Mathf.Clamp01(speed / 3.2f);
            position.y += Mathf.Abs(Mathf.Sin(cycle)) * walkBobHeight * stride;
            var localVelocity = transform.InverseTransformDirection(flatVelocity.normalized);
            var pitch = Mathf.Sin(cycle) * (quadruped ? 3.2f : 1.5f) * stride;
            var roll = -localVelocity.x * moveLean * stride;
            rotation *= Quaternion.Euler(pitch, 0f, roll);
        }

        private void ApplyPose(ref Vector3 position, ref Quaternion rotation)
        {
            if (currentPose == Pose.None)
            {
                return;
            }

            if (currentPose == Pose.Dead)
            {
                position.y -= quadruped ? 0.42f : 0.72f;
                rotation *= quadruped
                    ? Quaternion.Euler(0f, 0f, 86f)
                    : Quaternion.Euler(0f, 0f, 92f);
                return;
            }

            var normalized = Mathf.Clamp01((Time.time - poseStartedAt) / Mathf.Max(0.01f, poseLength));
            var arc = Mathf.Sin(normalized * Mathf.PI);
            switch (currentPose)
            {
                case Pose.Attack:
                    position.z += arc * (quadruped ? 0.22f : 0.16f);
                    position.y -= arc * (quadruped ? 0.06f : 0.03f);
                    rotation *= Quaternion.Euler(-arc * (quadruped ? 11f : 7f), arc * 10f, arc * 5f);
                    break;
                case Pose.Hit:
                    position.z -= arc * 0.14f;
                    rotation *= Quaternion.Euler(arc * 8f, -arc * 16f, arc * 9f);
                    break;
                case Pose.Stun:
                    position.y -= arc * 0.08f;
                    rotation *= Quaternion.Euler(arc * 10f, 0f, -arc * 12f);
                    break;
            }

            if (normalized >= 1f)
            {
                currentPose = Pose.None;
            }
        }

        private void BeginPose(Pose pose, float duration)
        {
            if (currentPose == Pose.Dead)
            {
                return;
            }

            currentPose = pose;
            poseStartedAt = Time.time;
            poseLength = duration;
        }

        private void HandleAttackStarted()
        {
            BeginPose(Pose.Attack, attackDuration);
        }

        private void HandleStunned(float duration)
        {
            BeginPose(Pose.Stun, Mathf.Max(stunDuration, duration));
        }

        private void HandleDamaged(Health damagedHealth, float amount, GameObject source)
        {
            BeginPose(Pose.Hit, hitDuration);
        }

        private void HandleDied(Health deadHealth, GameObject source)
        {
            currentPose = Pose.Dead;
            poseStartedAt = Time.time;
            poseLength = 999f;
        }
    }
}
