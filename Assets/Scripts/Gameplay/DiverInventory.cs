using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ballast.Gameplay
{
    public class DiverInventory : MonoBehaviour
    {
        [SerializeField] private int maxSlots = 6;
        [SerializeField] private Transform orbitRoot;
        [SerializeField] private LayerMask orbitItemMask;
        [SerializeField] private string orbitItemLayerName = "OrbitItem";
        [SerializeField] private float dropRayDistance = 50f;
        [SerializeField] private Camera dropCamera;

        private readonly List<ItemPickup> items = new();
        private int orbitItemLayer = -1;

        public IReadOnlyList<ItemPickup> Items => items;
        public int MaxSlots => maxSlots;
        public Transform OrbitRoot => orbitRoot;

        public int ManifestItemCount
        {
            get
            {
                int c = 0;
                for (int i = 0; i < items.Count; i++)
                    if (items[i] != null && items[i].Data != null && items[i].Data.IsManifestItem) c++;
                return c;
            }
        }

        public event Action<ItemPickup> OnCollected;
        public event Action<ItemPickup> OnDropped;

        private void Awake()
        {
            if (dropCamera == null) dropCamera = Camera.main;
            orbitItemLayer = LayerMask.NameToLayer(orbitItemLayerName);
        }

        private void Update()
        {
            if (TryReadTap(out Vector2 screenPos)) TryDropAt(screenPos);
        }

        private static bool TryReadTap(out Vector2 screenPos)
        {
            screenPos = default;
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPos = mouse.position.ReadValue();
                return true;
            }
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = touch.primaryTouch.position.ReadValue();
                return true;
            }
            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<ItemPickup>(out var pickup)) return;
            if (items.Contains(pickup)) return;
            TryCollect(pickup);
        }

        public bool TryCollect(ItemPickup pickup)
        {
            if (pickup == null || pickup.Data == null) return false;
            if (items.Count >= maxSlots) return false;

            var ws = WeightSystem.Instance;

            items.Add(pickup);
            pickup.PlayPickupSfx();
            if (orbitItemLayer >= 0) SetLayerRecursive(pickup.gameObject, orbitItemLayer);
            if (orbitRoot != null) pickup.transform.SetParent(orbitRoot, worldPositionStays: false);
            ws?.AddWeight(pickup.Data.Weight);
            OnCollected?.Invoke(pickup);
            return true;
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            var t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursive(t.GetChild(i).gameObject, layer);
        }

        public bool TryDrop(ItemPickup pickup)
        {
            if (pickup == null) return false;
            int idx = items.IndexOf(pickup);
            if (idx < 0) return false;

            items.RemoveAt(idx);
            pickup.transform.SetParent(null, worldPositionStays: true);
            WeightSystem.Instance?.RemoveWeight(pickup.Data.Weight);
            OnDropped?.Invoke(pickup);
            Destroy(pickup.gameObject);
            return true;
        }

        private void TryDropAt(Vector2 screenPos)
        {
            if (dropCamera == null || items.Count == 0) return;
            Ray ray = dropCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, dropRayDistance, orbitItemMask, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.TryGetComponent<ItemPickup>(out var pickup))
                {
                    TryDrop(pickup);
                }
            }
        }
    }
}
