using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ballast.Gameplay
{
    public class InventoryStrip : MonoBehaviour
    {
        [SerializeField] private ManifestRegistry registry;
        [SerializeField] private Transform slotParent;
        [SerializeField] private Image slotPrefab;
        [SerializeField] private Color heldColor = Color.white;
        [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private bool manifestItemsOnly = true;

        private readonly Dictionary<ItemData, Image> slotsByItem = new();
        private readonly Dictionary<ItemData, int> countsByItem = new();

        private DiverInventory inventory;
        private bool subscribed;

        private void Awake()
        {
            BuildSlots();
        }

        private void Start()
        {
            TrySubscribe();
        }

        private void Update()
        {
            if (!subscribed) TrySubscribe();
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.OnCollected -= HandleCollected;
                inventory.OnDropped -= HandleDropped;
            }
        }

        private void BuildSlots()
        {
            if (registry == null || slotPrefab == null || slotParent == null) return;
            for (int i = slotParent.childCount - 1; i >= 0; i--) Destroy(slotParent.GetChild(i).gameObject);
            slotsByItem.Clear();
            countsByItem.Clear();

            var items = registry.Items;
            for (int i = 0; i < items.Count; i++)
            {
                var data = items[i];
                if (data == null) continue;
                if (manifestItemsOnly && !data.IsManifestItem) continue;
                if (slotsByItem.ContainsKey(data)) continue;

                var slot = Instantiate(slotPrefab, slotParent);
                slot.sprite = data.Icon;
                slot.enabled = data.Icon != null;
                slot.color = emptyColor;
                slotsByItem[data] = slot;
                countsByItem[data] = 0;
            }
        }

        private void TrySubscribe()
        {
            if (subscribed) return;
            if (inventory == null) inventory = FindFirstObjectByType<DiverInventory>();
            if (inventory == null) return;
            inventory.OnCollected += HandleCollected;
            inventory.OnDropped += HandleDropped;
            subscribed = true;
        }

        private void HandleCollected(ItemPickup pickup)
        {
            if (pickup == null || pickup.Data == null) return;
            var data = pickup.Data;
            if (!slotsByItem.TryGetValue(data, out var slot)) return;
            int count = countsByItem.TryGetValue(data, out var c) ? c : 0;
            count++;
            countsByItem[data] = count;
            if (count == 1) slot.color = heldColor;
        }

        private void HandleDropped(ItemPickup pickup)
        {
            if (pickup == null || pickup.Data == null) return;
            var data = pickup.Data;
            if (!slotsByItem.TryGetValue(data, out var slot)) return;
            int count = countsByItem.TryGetValue(data, out var c) ? c : 0;
            count = Mathf.Max(0, count - 1);
            countsByItem[data] = count;
            if (count == 0) slot.color = emptyColor;
        }
    }
}
