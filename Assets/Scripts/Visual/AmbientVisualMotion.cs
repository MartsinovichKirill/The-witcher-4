using UnityEngine;

namespace WitcherRightVersion.Visual
{
    public sealed class AmbientVisualMotion : MonoBehaviour
    {
        [SerializeField] private Vector3 bobAmplitude = new Vector3(0.08f, 0.22f, 0.08f);
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 24f, 0f);
        [SerializeField] private float motionSpeed = 1.2f;
        [SerializeField] private float scalePulse = 0.08f;
        [SerializeField] private float lightPulse = 0.2f;

        private Vector3 basePosition;
        private Vector3 baseScale;
        private Light attachedLight;
        private float baseLightIntensity;
        private float phase;

        private void Awake()
        {
            basePosition = transform.localPosition;
            baseScale = transform.localScale;
            attachedLight = GetComponentInChildren<Light>();
            baseLightIntensity = attachedLight != null ? attachedLight.intensity : 0f;
            phase = Mathf.Abs(gameObject.name.GetHashCode() % 997) / 997f * Mathf.PI * 2f;
        }

        private void Update()
        {
            var time = Time.time * motionSpeed + phase;
            transform.localPosition = basePosition + new Vector3(
                Mathf.Sin(time * 0.73f) * bobAmplitude.x,
                Mathf.Sin(time) * bobAmplitude.y,
                Mathf.Cos(time * 0.61f) * bobAmplitude.z);

            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);

            var pulse = 1f + Mathf.Sin(time * 1.37f) * scalePulse;
            transform.localScale = baseScale * pulse;

            if (attachedLight != null)
            {
                attachedLight.intensity = Mathf.Max(0f, baseLightIntensity * (1f + Mathf.Sin(time * 1.83f) * lightPulse));
            }
        }

        public void Configure(Vector3 newBobAmplitude, Vector3 newRotationSpeed, float newMotionSpeed, float newScalePulse, float newLightPulse)
        {
            bobAmplitude = newBobAmplitude;
            rotationSpeed = newRotationSpeed;
            motionSpeed = Mathf.Max(0.05f, newMotionSpeed);
            scalePulse = Mathf.Max(0f, newScalePulse);
            lightPulse = Mathf.Max(0f, newLightPulse);
        }
    }
}
