using UnityEngine;

namespace Ballast.Gameplay
{
    [RequireComponent(typeof(RectTransform))]
    public class UIFloat : MonoBehaviour
    {
        [SerializeField] private float amplitude = 8f;
        [SerializeField] private float frequency = 0.4f;
        [SerializeField] private float phaseOffset = 0f;
        [SerializeField] private bool randomizePhase = true;
        [SerializeField] private bool autoStart = true;

        private RectTransform rt;
        private Vector2 basePos;
        private bool floating;

        private void Awake()
        {
            rt = (RectTransform)transform;
            if (randomizePhase && Mathf.Approximately(phaseOffset, 0f))
                phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void OnEnable()
        {
            if (autoStart && GetComponent<UIFallIn>() == null) BeginFloating();
        }

        public void BeginFloating()
        {
            basePos = rt.anchoredPosition;
            floating = true;
        }

        public void Stop() => floating = false;

        private void Update()
        {
            if (!floating) return;
            float y = Mathf.Sin(Time.time * Mathf.PI * 2f * frequency + phaseOffset) * amplitude;
            rt.anchoredPosition = basePos + new Vector2(0f, y);
        }
    }
}
