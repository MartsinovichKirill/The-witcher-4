using UnityEngine;

namespace WitcherRightVersion.Combat
{
    [RequireComponent(typeof(Health))]
    public sealed class CombatVisualFeedback : MonoBehaviour
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        [SerializeField] private Color hitFlashColor = new Color(1f, 0.2f, 0.12f, 1f);
        [SerializeField] private Color deathColor = new Color(0.12f, 0.08f, 0.07f, 1f);
        [SerializeField] private float flashDuration = 0.16f;

        private Health health;
        private Renderer[] renderers;
        private MaterialPropertyBlock propertyBlock;
        private float flashUntil;
        private bool flashing;
        private bool dead;

        private void Awake()
        {
            health = GetComponent<Health>();
            renderers = GetComponentsInChildren<Renderer>(true);
            propertyBlock = new MaterialPropertyBlock();

            health.Damaged += HandleDamaged;
            health.Died += HandleDied;
        }

        private void Update()
        {
            if (flashing && !dead && Time.time >= flashUntil)
            {
                flashing = false;
                ClearColor();
            }
        }

        private void OnDestroy()
        {
            if (health == null)
            {
                return;
            }

            health.Damaged -= HandleDamaged;
            health.Died -= HandleDied;
        }

        public void Configure(Color newHitFlashColor, Color newDeathColor, float newFlashDuration = 0.16f)
        {
            hitFlashColor = newHitFlashColor;
            deathColor = newDeathColor;
            flashDuration = Mathf.Max(0.04f, newFlashDuration);
        }

        private void HandleDamaged(Health damagedHealth, float amount, GameObject source)
        {
            if (dead)
            {
                return;
            }

            flashing = true;
            flashUntil = Time.time + flashDuration;
            ApplyColor(hitFlashColor);
        }

        private void HandleDied(Health deadHealth, GameObject source)
        {
            dead = true;
            flashing = false;
            ApplyColor(deathColor);
        }

        private void ApplyColor(Color color)
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                var target = renderers[i];
                if (target == null || target.sharedMaterial == null || !target.sharedMaterial.HasProperty(ColorProperty))
                {
                    continue;
                }

                target.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(ColorProperty, color);
                target.SetPropertyBlock(propertyBlock);
            }
        }

        private void ClearColor()
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].SetPropertyBlock(null);
                }
            }
        }
    }
}
