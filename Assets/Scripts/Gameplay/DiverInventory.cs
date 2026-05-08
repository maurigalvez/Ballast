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
        [SerializeField] private float dropRayDistance = 50f;
        [SerializeField] private float dropUpSpeed = 2.5f;
        [SerializeField] private float dropFadeDuration = 2.5f;
        [SerializeField] private Camera dropCamera;

        private readonly List<ItemPickup> items = new();

        public IReadOnlyList<ItemPickup> Items => items;
        public int MaxSlots => maxSlots;
        public Transform OrbitRoot => orbitRoot;

        public event Action<ItemPickup> OnCollected;
        public event Action<ItemPickup> OnDropped;

        private void Awake()
        {
            if (dropCamera == null) dropCamera = Camera.main;
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
            if (ws != null && ws.CurrentWeight + pickup.Data.Weight > ws.MaxWeight) return false;

            items.Add(pickup);
            pickup.DisableTrigger();
            if (orbitRoot != null) pickup.transform.SetParent(orbitRoot, worldPositionStays: false);
            ws?.AddWeight(pickup.Data.Weight);
            OnCollected?.Invoke(pickup);
            return true;
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
            pickup.StartCoroutine(pickup.DropAndFade(dropUpSpeed, dropFadeDuration));
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
