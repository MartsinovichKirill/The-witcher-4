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
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 65f;
        [SerializeField] private float mouseSensitivity = 2.4f;
        [SerializeField] private float followSmoothTime = 0.08f;

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

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var targetPosition = target.position + targetOffset;
            var desiredPosition = targetPosition - rotation * Vector3.forward * distance;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, followSmoothTime);
            transform.rotation = rotation;
        }
    }
}

