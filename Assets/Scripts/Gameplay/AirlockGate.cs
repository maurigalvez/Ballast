using System.Collections.Generic;
using UnityEngine;

namespace Ballast.Gameplay
{
    public class AirlockGate : WeightGate
    {
        [Header("Airlock")]
        [SerializeField] private DiverInventory diverInventory;
        [SerializeField] private int requiredManifest = 3;

        private bool resolved;

        protected override void Start()
        {
            base.Start();
            if (label != null) label.text = $"MANIFEST {requiredManifest}";
        }

        protected override void HandleDiverEntered(Collider diver)
        {
            if (resolved) return;
            resolved = true;

            var delivered = new List<ItemData>();
            if (diverInventory != null)
            {
                var snapshot = new List<ItemPickup>(diverInventory.Items);
                for (int i = 0; i < snapshot.Count; i++)
                {
                    var pickup = snapshot[i];
                    if (pickup == null || pickup.Data == null) continue;
                    if (pickup.Data.IsManifestItem) delivered.Add(pickup.Data);
                    else diverInventory.TryDrop(pickup);
                }
            }

            if (GameManager.Instance != null)
                GameManager.Instance.SetDeliveredManifest(delivered);

            int n = delivered.Count;

            if (n >= requiredManifest)
            {
                Open();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetLastManifestCount(n);
                    GameManager.Instance.RunEnd(RunEndReason.Success);
                }
            }
            else
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetLastManifestCount(n);
                    GameManager.Instance.FlagAirlockManifestFail();
                    GameManager.Instance.RunEnd(RunEndReason.ManifestFailed);
                }
            }
        }
    }
}
