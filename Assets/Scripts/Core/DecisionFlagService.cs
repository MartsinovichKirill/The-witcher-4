using System.Collections.Generic;
using UnityEngine;

namespace WitcherRightVersion.Core
{
    public sealed class DecisionFlagService : MonoBehaviour
    {
        private readonly HashSet<string> flags = new HashSet<string>();

        public static DecisionFlagService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void SetFlag(string flag)
        {
            if (string.IsNullOrWhiteSpace(flag))
            {
                return;
            }

            flags.Add(flag);
            Debug.Log($"Decision flag set: {flag}", this);
        }

        public bool HasFlag(string flag)
        {
            return !string.IsNullOrWhiteSpace(flag) && flags.Contains(flag);
        }
    }
}
