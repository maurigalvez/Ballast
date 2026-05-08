using System.Collections.Generic;
using UnityEngine;

namespace Ballast.Gameplay
{
    public class OrbitSystem : MonoBehaviour
    {
        [SerializeField] private DiverInventory inventory;
        [SerializeField] private float radius = 1.2f;
        [SerializeField] private float yOffset = 0.3f;
        [SerializeField] private float orbitSpeedDeg = 30f;
        [SerializeField] private float spinSpeedDeg = 90f;
        [SerializeField] private float reflowDuration = 0.3f;
        [SerializeField] private float appearScaleFrom = 0.8f;
        [SerializeField] private float appearScaleDuration = 0.15f;

        private class Slot
        {
            public ItemPickup pickup;
            public float startAngle;
            public float currentAngle;
            public float targetAngle;
            public float reflowT;
            public float appearT;
        }

        private readonly List<Slot> slots = new();
        private float globalAngle;

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.OnCollected += HandleCollected;
                inventory.OnDropped += HandleDropped;
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.OnCollected -= HandleCollected;
                inventory.OnDropped -= HandleDropped;
            }
        }

        private void HandleCollected(ItemPickup pickup)
        {
            slots.Add(new Slot
            {
                pickup = pickup,
                startAngle = 0f,
                currentAngle = 0f,
                targetAngle = 0f,
                reflowT = 0f,
                appearT = 0f
            });
            RecomputeTargets();
        }

        private void HandleDropped(ItemPickup pickup)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].pickup == pickup) { slots.RemoveAt(i); break; }
            }
            RecomputeTargets();
        }

        private void RecomputeTargets()
        {
            int n = slots.Count;
            if (n == 0) return;
            float step = 360f / n;
            for (int i = 0; i < n; i++)
            {
                var s = slots[i];
                s.startAngle = s.currentAngle;
                s.targetAngle = step * i;
                s.reflowT = 0f;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            globalAngle = Mathf.Repeat(globalAngle + orbitSpeedDeg * dt, 360f);

            for (int i = 0; i < slots.Count; i++)
            {
                var s = slots[i];
                if (s.pickup == null) continue;

                if (s.reflowT < reflowDuration)
                {
                    s.reflowT = Mathf.Min(s.reflowT + dt, reflowDuration);
                    float k = reflowDuration > 0f ? s.reflowT / reflowDuration : 1f;
                    s.currentAngle = Mathf.LerpAngle(s.startAngle, s.targetAngle, k);
                }
                else
                {
                    s.currentAngle = s.targetAngle;
                }

                float ang = (globalAngle + s.currentAngle) * Mathf.Deg2Rad;
                Vector3 localPos = new(Mathf.Cos(ang) * radius, yOffset, Mathf.Sin(ang) * radius);
                s.pickup.transform.localPosition = localPos;
                s.pickup.transform.Rotate(0f, spinSpeedDeg * dt, 0f, Space.Self);

                s.appearT = Mathf.Min(s.appearT + dt, appearScaleDuration);
                float a = appearScaleDuration > 0f ? s.appearT / appearScaleDuration : 1f;
                float scale = Mathf.Lerp(appearScaleFrom, 1f, a);
                s.pickup.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}
