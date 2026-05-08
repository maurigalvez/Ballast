using System.Collections;
using UnityEngine;

namespace Ballast.Gameplay
{
    public class DiverFeedback : MonoBehaviour
    {
        [SerializeField] private DiverInventory inventory;
        [SerializeField] private Transform visual;
        [SerializeField] private float pulsePeak = 1.08f;
        [SerializeField] private float pulseDuration = 0.2f;

        private Coroutine running;
        private Vector3 baseScale = Vector3.one;

        private void Awake()
        {
            if (visual != null) baseScale = visual.localScale;
        }

        private void OnEnable()
        {
            if (inventory != null) inventory.OnCollected += HandleCollected;
        }

        private void OnDisable()
        {
            if (inventory != null) inventory.OnCollected -= HandleCollected;
        }

        private void HandleCollected(ItemPickup _)
        {
            if (visual == null) return;
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Pulse());
        }

        private IEnumerator Pulse()
        {
            float t = 0f;
            while (t < pulseDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / pulseDuration));
                float s = Mathf.Lerp(1f, pulsePeak, k);
                visual.localScale = baseScale * s;
                yield return null;
            }
            visual.localScale = baseScale;
            running = null;
        }
    }
}
