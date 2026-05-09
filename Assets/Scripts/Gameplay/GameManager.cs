using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ballast.Gameplay
{
    public enum RunEndReason { O2Empty, CeilingCaught, Success, ManifestFailed }
    public enum RunState { PreRun, Running, Ended }

    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private Rigidbody diverRigidbody;

        public RunState State { get; private set; } = RunState.PreRun;
        public bool RunEnded => State == RunState.Ended;
        public int LastManifestCount { get; private set; }
        public float RunTimeSeconds { get; private set; }
        public IReadOnlyList<ItemData> DeliveredManifest { get; private set; } = Array.Empty<ItemData>();

        public event Action OnRunStart;
        public event Action<RunEndReason> OnRunEnd;
        public event Action<WeightGate> OnGateApproach;

        private bool airlockManifestFailFlag;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (OxygenSystem.Instance != null) OxygenSystem.Instance.OnO2Empty -= HandleO2Empty;
        }

        private void Start()
        {
            if (OxygenSystem.Instance != null)
                OxygenSystem.Instance.OnO2Empty += HandleO2Empty;
        }

        private void Update()
        {
            if (State == RunState.Running)
                RunTimeSeconds += Time.deltaTime;
        }

        public void StartRun()
        {
            if (State != RunState.PreRun) return;
            State = RunState.Running;
            if (OxygenSystem.Instance != null) OxygenSystem.Instance.IsDepleting = true;
            OnRunStart?.Invoke();
        }

        private void HandleO2Empty()
        {
            RunEnd(airlockManifestFailFlag ? RunEndReason.ManifestFailed : RunEndReason.O2Empty);
        }

        public void FlagAirlockManifestFail()
        {
            airlockManifestFailFlag = true;
        }

        public void NotifyGateApproach(WeightGate gate)
        {
            OnGateApproach?.Invoke(gate);
        }

        public void SetLastManifestCount(int count)
        {
            LastManifestCount = count;
        }

        public void SetDeliveredManifest(IReadOnlyList<ItemData> delivered)
        {
            DeliveredManifest = delivered ?? Array.Empty<ItemData>();
        }

        public void RunEnd(RunEndReason reason)
        {
            if (State == RunState.Ended) return;
            State = RunState.Ended;

            if (OxygenSystem.Instance != null) OxygenSystem.Instance.IsDepleting = false;
            if (diverRigidbody != null)
            {
                diverRigidbody.linearVelocity = Vector3.zero;
                diverRigidbody.isKinematic = true;
            }
            OnRunEnd?.Invoke(reason);
        }
    }
}
