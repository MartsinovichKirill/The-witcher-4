using System;
using UnityEngine;

namespace WitcherRightVersion.Combat
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private string displayName = "Target";
        [SerializeField] private float maxHealth = 100f;

        private float currentHealth;
        private bool isDead;

        public event Action<Health, GameObject> Died;

        public string DisplayName => displayName;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void Configure(string newDisplayName, float newMaxHealth)
        {
            displayName = newDisplayName;
            maxHealth = Mathf.Max(1f, newMaxHealth);
            currentHealth = maxHealth;
            isDead = false;
        }

        public void TakeDamage(float amount, GameObject source)
        {
            if (isDead || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            Debug.Log($"{displayName} took {amount:0} damage. HP: {currentHealth:0}/{maxHealth:0}", this);

            if (currentHealth <= 0f)
            {
                Die(source);
            }
        }

        public void Heal(float amount)
        {
            if (isDead || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        private void Die(GameObject source)
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            Debug.Log($"{displayName} died.", this);
            Died?.Invoke(this, source);
        }
    }
}
