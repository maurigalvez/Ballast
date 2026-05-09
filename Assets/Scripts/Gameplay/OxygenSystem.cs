using System;
using UnityEngine;

namespace Ballast.Gameplay
{
    [DefaultExecutionOrder(-100)]
    public class OxygenSystem : MonoBehaviour
    {
        public static OxygenSystem Instance { get; private set; }

        [SerializeField] private float maxO2 = 100f;
        [SerializeField] private float depletionPerSec = 1.5f;
        [SerializeField] private float currentO2;
        [SerializeField, Range(0f, 1f)] private float warningThreshold = 0.25f;
        [SerializeField, Range(0f, 1f)] private float criticalThreshold = 0.10f;

        public float CurrentO2 => currentO2;
        public float MaxO2 => maxO2;
        public float Percent => maxO2 > 0f ? currentO2 / maxO2 : 0f;
        public float WarningThreshold => warningThreshold;
        public float CriticalThreshold => criticalThreshold;
        public bool IsDepleting { get; set; } = false;

        public event Action<float> OnO2Changed;
        public event Action OnO2Warning;
        public event Action OnO2Critical;
        public event Action OnO2Empty;

        private bool warningFired;
        private bool criticalFired;
        private bool emptyFired;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            currentO2 = maxO2;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            OnO2Changed?.Invoke(Percent);
        }

        private void Update()
        {
            if (!IsDepleting || emptyFired) return;
            if (depletionPerSec > 0f) Drain(depletionPerSec * Time.deltaTime);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || emptyFired) return;
            Drain(amount);
        }

        private void Drain(float amount)
        {
            float next = Mathf.Max(0f, currentO2 - amount);
            if (Mathf.Approximately(next, currentO2)) return;
            currentO2 = next;
            float percent = Percent;
            OnO2Changed?.Invoke(percent);
            if (!warningFired && percent <= warningThreshold)
            {
                warningFired = true;
                OnO2Warning?.Invoke();
            }
            if (!criticalFired && percent <= criticalThreshold)
            {
                criticalFired = true;
                OnO2Critical?.Invoke();
            }
            if (currentO2 <= 0f && !emptyFired)
            {
                emptyFired = true;
                OnO2Empty?.Invoke();
            }
        }
    }
}
