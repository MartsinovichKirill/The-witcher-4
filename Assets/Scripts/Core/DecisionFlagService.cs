using System.Collections.Generic;
using System.Linq;
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

        public string[] CaptureFlags()
        {
            return flags.OrderBy(flag => flag).ToArray();
        }

        public void RestoreFlags(IEnumerable<string> savedFlags)
        {
            flags.Clear();

            if (savedFlags == null)
            {
                return;
            }

            foreach (var flag in savedFlags)
            {
                if (!string.IsNullOrWhiteSpace(flag))
                {
                    flags.Add(flag);
                }
            }

            Debug.Log($"Decision flags restored: {flags.Count}", this);
        }
    }
}
