using UnityEngine;

namespace WitcherRightVersion.Player
{
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.55f, 0f);

        [Header("Orbit")]
        [SerializeField] private float distance = 5.5f;
        [SerializeField] private float minDistance = 3.2f;
        [SerializeField] private float maxDistance = 7.4f;
        [SerializeField] private float zoomSensitivity = 1.4f;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 65f;
        [SerializeField] private float mouseSensitivity = 2.4f;
        [SerializeField] private float followSmoothTime = 0.08f;
        [SerializeField] private float collisionRadius = 0.22f;
        [SerializeField] private LayerMask collisionMask = ~0;

        private Vector3 smoothVelocity;
        private float yaw;
        private float pitch = 18f;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target != null)
            {
                yaw = target.eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                distance = Mathf.Clamp(distance - scroll * zoomSensitivity, minDistance, maxDistance);
            }

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var targetPosition = target.position + targetOffset;
            var desiredDistance = GetCollisionAdjustedDistance(targetPosition, rotation);
            var desiredPosition = targetPosition - rotation * Vector3.forward * desiredDistance;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, followSmoothTime);
            transform.rotation = rotation;
        }

        private float GetCollisionAdjustedDistance(Vector3 targetPosition, Quaternion rotation)
        {
            var direction = -(rotation * Vector3.forward);
            if (Physics.SphereCast(targetPosition, collisionRadius, direction, out var hit, distance, collisionMask, QueryTriggerInteraction.Ignore)
                && target != null
                && !hit.transform.IsChildOf(target))
            {
                return Mathf.Clamp(hit.distance - collisionRadius, minDistance * 0.45f, distance);
            }

            return distance;
        }
    }
}
