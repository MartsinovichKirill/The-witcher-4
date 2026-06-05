using UnityEngine;
using WitcherRightVersion.Combat;
using WitcherRightVersion.Dialogue;

namespace WitcherRightVersion.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float runSpeed = 6.25f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float acceleration = 16f;
        [SerializeField] private float deceleration = 20f;
        [SerializeField] private float gravity = -24f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController characterController;
        private Health health;
        private CombatController combat;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;

        public bool IsMoving { get; private set; }
        public bool IsRunning { get; private set; }
        public Vector3 MoveDirection { get; private set; }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            combat = GetComponent<CombatController>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (health != null && health.IsDead)
            {
                StopHorizontalMovement();
                return;
            }

            if (DialogueService.Instance != null && DialogueService.Instance.IsDialogueOpen)
            {
                StopHorizontalMovement();
                return;
            }

            if (combat != null && combat.IsDodging)
            {
                StopHorizontalMovement();
                return;
            }

            Move();
        }

        private void Move()
        {
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");
            var input = new Vector3(horizontal, 0f, vertical);
            input = Vector3.ClampMagnitude(input, 1f);

            IsMoving = input.sqrMagnitude > 0.01f;
            IsRunning = IsMoving && Input.GetKey(KeyCode.LeftShift);

            var moveDirection = GetCameraRelativeDirection(input);
            var speed = IsRunning ? runSpeed : walkSpeed;
            var desiredHorizontalVelocity = moveDirection * speed;
            var smoothRate = IsMoving ? acceleration : deceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredHorizontalVelocity, smoothRate * Time.deltaTime);
            MoveDirection = horizontalVelocity.sqrMagnitude > 0.01f ? horizontalVelocity.normalized : Vector3.zero;

            if (MoveDirection.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(MoveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            var velocity = horizontalVelocity;
            velocity.y = verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);
        }

        private void StopHorizontalMovement()
        {
            IsMoving = false;
            IsRunning = false;
            MoveDirection = Vector3.zero;
            horizontalVelocity = Vector3.zero;
        }

        private Vector3 GetCameraRelativeDirection(Vector3 input)
        {
            if (!IsMoving)
            {
                return Vector3.zero;
            }

            if (cameraTransform == null)
            {
                return input.normalized;
            }

            var forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            var right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            return (forward * input.z + right * input.x).normalized;
        }
    }
}
