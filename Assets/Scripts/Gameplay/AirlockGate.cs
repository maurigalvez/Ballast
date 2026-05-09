using UnityEngine;

namespace Ballast.Gameplay
{
    public class AirlockGate : WeightGate
    {
        [Header("Airlock")]
        [SerializeField] private DiverInventory diverInventory;
        [SerializeField] private int requiredManifest = 3;

        private bool resolved;

        protected override void HandleDiverEntered(Collider diver)
        {
            if (resolved) return;
            resolved = true;

            if (diverInventory != null)
            {
                var snapshot = new System.Collections.Generic.List<ItemPickup>(diverInventory.Items);
                for (int i = 0; i < snapshot.Count; i++)
                {
                    var pickup = snapshot[i];
                    if (pickup == null || pickup.Data == null) continue;
                    if (!pickup.Data.IsManifestItem) diverInventory.TryDrop(pickup);
                }
            }

            int n = diverInventory != null ? diverInventory.ManifestItemCount : 0;

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
                GameManager.Instance?.FlagAirlockManifestFail();
                resolved = false;
            }
        }
    }
}