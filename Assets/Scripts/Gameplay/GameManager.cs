using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ballast.Gameplay
{
    public enum RunEndReason { O2Empty, CeilingCaught, Success, ManifestFailed }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private Rigidbody diverRigidbody;
        [SerializeField] private float fadeDuration = 1.5f;

        public bool RunEnded { get; private set; }
        public int LastManifestCount { get; private set; }

        public event Action<RunEndReason> OnRunEnd;
        public event Action<WeightGate> OnGateApproach;

        private bool airlockManifestFailFlag;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (OxygenSystem.Instance != null) OxygenSystem.Instance.OnO2Empty -= HandleO2Empty;
        }

        private void Start()
        {
            if (OxygenSystem.Instance != null)
                OxygenSystem.Instance.OnO2Empty += HandleO2Empty;
        }

        private void HandleO2Empty()
        {
            RunEnd(airlockManifestFailFlag ? RunEndReason.ManifestFailed : RunEndReason.O2Empty);
        }

        public void FlagAirlockManifestFail()
        {
            airlockManifestFailFlag = true;
        }

        public void NotifyGateApproach(WeightGate gate)
        {
            OnGateApproach?.Invoke(gate);
        }

        public void SetLastManifestCount(int count)
        {
            LastManifestCount = count;
        }

        public void RunEnd(RunEndReason reason)
        {
            if (RunEnded) return;
            RunEnded = true;

            if (OxygenSystem.Instance != null) OxygenSystem.Instance.IsDepleting = false;
            if (diverRigidbody != null)
            {
                diverRigidbody.linearVelocity = Vector3.zero;
                diverRigidbody.isKinematic = true;
            }

            OnRunEnd?.Invoke(reason);
            StartCoroutine(FadeAndShow(reason));
        }

        private IEnumerator FadeAndShow(RunEndReason reason)
        {
            var canvasGo = new GameObject("RunEndCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var imgGo = new GameObject("Fade");
            imgGo.transform.SetParent(canvasGo.transform, false);
            var img = imgGo.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);
            var imgRt = img.rectTransform;
            imgRt.anchorMin = Vector2.zero;
            imgRt.anchorMax = Vector2.one;
            imgRt.offsetMin = Vector2.zero;
            imgRt.offsetMax = Vector2.zero;

            var textGo = new GameObject("Message");
            textGo.transform.SetParent(canvasGo.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = MessageFor(reason);
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 64f;
            text.color = new Color(1f, 1f, 1f, 0f);
            var textRt = text.rectTransform;
            textRt.anchorMin = new Vector2(0.1f, 0.4f);
            textRt.anchorMax = new Vector2(0.9f, 0.6f);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / fadeDuration);
                img.color = new Color(0f, 0f, 0f, a);
                text.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
            img.color = Color.black;
            text.color = Color.white;
        }

        private string MessageFor(RunEndReason reason)
        {
            switch (reason)
            {
                case RunEndReason.Success: return $"ESCAPED — {LastManifestCount} manifest items";
                case RunEndReason.ManifestFailed: return "MANIFEST INCOMPLETE";
                case RunEndReason.CeilingCaught: return "CRUSHED";
                case RunEndReason.O2Empty:
                default: return "OUT OF OXYGEN";
            }
        }
    }
}
