using System;
using UnityEngine;

namespace Ballast.Gameplay
{
    public enum WeightZone { Light, Loaded, Critical }

    [DefaultExecutionOrder(-100)]
    public class WeightSystem : MonoBehaviour
    {
        public static WeightSystem Instance { get; private set; }

        [SerializeField] private float maxWeight = 10f;
        [SerializeField] private float loadedThreshold = 3.3f;
        [SerializeField] private float criticalThreshold = 6.6f;
        [SerializeField] private float currentWeight;

        public float CurrentWeight => currentWeight;
        public float MaxWeight => maxWeight;
        public WeightZone Zone { get; private set; }

        public event Action<float> OnWeightChanged;
        public event Action<WeightZone> OnZoneChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            RecalculateZone(force: true);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void AddWeight(float amount)
        {
            if (amount <= 0f) return;
            SetWeight(currentWeight + amount);
        }

        public void RemoveWeight(float amount)
        {
            if (amount <= 0f) return;
            SetWeight(currentWeight - amount);
        }

        private void SetWeight(float value)
        {
            float clamped = Mathf.Max(value, 0f);
            if (Mathf.Approximately(clamped, currentWeight)) return;
            currentWeight = clamped;
            OnWeightChanged?.Invoke(currentWeight);
            RecalculateZone();
        }

        private void RecalculateZone(bool force = false)
        {
            WeightZone next =
                currentWeight < loadedThreshold ? WeightZone.Light :
                currentWeight < criticalThreshold ? WeightZone.Loaded :
                WeightZone.Critical;

            if (force || next != Zone)
            {
                Zone = next;
                OnZoneChanged?.Invoke(Zone);
            }
        }
    }
}
