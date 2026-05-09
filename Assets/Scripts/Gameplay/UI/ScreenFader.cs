using System.Collections;
using UnityEngine;

namespace Ballast.Gameplay
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [SerializeField] private float defaultDuration = 0.5f;
        [SerializeField] private bool startOpaque = true;

        private CanvasGroup group;

        public float DefaultDuration => defaultDuration;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            group = GetComponent<CanvasGroup>();
            group.alpha = startOpaque ? 1f : 0f;
            group.blocksRaycasts = startOpaque;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public Coroutine FadeOut(float duration = -1f) => StartCoroutine(FadeTo(1f, duration < 0f ? defaultDuration : duration));
        public Coroutine FadeIn(float duration = -1f) => StartCoroutine(FadeTo(0f, duration < 0f ? defaultDuration : duration));

        public IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float start = group.alpha;
            float t = 0f;
            group.blocksRaycasts = true;
            if (duration <= 0f)
            {
                group.alpha = targetAlpha;
            }
            else
            {
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    group.alpha = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(t / duration));
                    yield return null;
                }
                group.alpha = targetAlpha;
            }
            group.blocksRaycasts = targetAlpha > 0.01f;
        }
    }
}
