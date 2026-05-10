using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Ballast.Gameplay
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class UIFallIn : MonoBehaviour
    {
        [SerializeField] private float fallDistance = 800f;
        [SerializeField] private float duration = 0.7f;
        [SerializeField] private float delay = 0f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private RectTransform rt;
        private UIFloat floater;

        private void Awake()
        {
            rt = (RectTransform)transform;
            floater = GetComponent<UIFloat>();
            if (floater != null) floater.Stop();
        }

        private void Start()
        {
            Debug.Log($"[UIFallIn] Start on {name}, starting coroutine. anchoredPos={((RectTransform)transform).anchoredPosition}", this);
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            // If a parent has a LayoutGroup, it will overwrite anchoredPosition every frame.
            // Bypass it for the duration of the fall via a LayoutElement with ignoreLayout.
            LayoutElement le = null;
            bool addedLayoutElement = false;
            bool prevIgnore = false;
            if (transform.parent != null && transform.parent.GetComponent<LayoutGroup>() != null)
            {
                le = GetComponent<LayoutElement>();
                if (le == null) { le = gameObject.AddComponent<LayoutElement>(); addedLayoutElement = true; }
                prevIgnore = le.ignoreLayout;
                le.ignoreLayout = true;
            }

            Vector2 target = rt.anchoredPosition;
            Vector2 start = target + new Vector2(0f, fallDistance);
            rt.anchoredPosition = start;
            Debug.Log($"[UIFallIn] {name} target={target} start={start} parentLayout={(transform.parent != null ? transform.parent.GetComponent<LayoutGroup>() : null)}", this);

            if (delay > 0f)
            {
                float d = 0f;
                while (d < delay) { d += Time.unscaledDeltaTime; yield return null; }
            }

            if (duration <= 0f)
            {
                rt.anchoredPosition = target;
            }
            else
            {
                // Skip the first frame's delta — it includes scene load time and would jump past duration.
                yield return null;
                float t = 0f;
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    float k = ease.Evaluate(Mathf.Clamp01(t / duration));
                    rt.anchoredPosition = Vector2.LerpUnclamped(start, target, k);
                    yield return null;
                }
                rt.anchoredPosition = target;
            }

            if (le != null)
            {
                if (addedLayoutElement) Destroy(le);
                else le.ignoreLayout = prevIgnore;
            }

            if (floater != null) floater.BeginFloating();
        }
    }
}
