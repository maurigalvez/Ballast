using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ballast.Gameplay
{
    public class HUDController : MonoBehaviour
    {
        [Header("Fill")]
        [SerializeField] private Image fillImage;
        [SerializeField, Min(0f)] private float smoothing = 6f;

        [Header("Label")]
        [SerializeField] private TMP_Text percentLabel;
        [SerializeField] private string percentFormat = "{0:0}%";

        [Header("Timer")]
        [SerializeField] private TMP_Text timerLabel;

        [Header("Weight")]
        [SerializeField] private TMP_Text weightLabel;
        [SerializeField] private string weightFormat = "{0:0} / {1:0} KG";
        [SerializeField] private Color weightColorLight = Color.white;
        [SerializeField] private Color weightColorMid = Color.yellow;
        [SerializeField] private Color weightColorMax = Color.red;

        [Header("Weight Pulse")]
        [SerializeField, Min(1f)] private float pulseScale = 1.3f;
        [SerializeField, Min(0.01f)] private float pulseDuration = 0.35f;
        [SerializeField] private AnimationCurve pulseCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.3f, 1f),
            new Keyframe(1f, 0f));
        [SerializeField] private Color pulseFlashColor = Color.white;
        [SerializeField, Range(0f, 1f)] private float pulseFlashAmount = 0.6f;

        [Header("Visibility")]
        [SerializeField] private GameObject root;
        [SerializeField] private bool hidePreRun = true;

        private float targetPercent = 1f;
        private float displayPercent = 1f;

        private bool subscribedOxygen;
        private bool subscribedGame;
        private bool subscribedWeight;

        private Vector3 weightLabelBaseScale = Vector3.one;
        private Color weightLabelBaseColor = Color.white;
        private float pulseTime = -1f;
        private bool weightInitialized;

        private void Awake()
        {
            if (weightLabel != null) weightLabelBaseScale = weightLabel.transform.localScale;
            TrySubscribe();
            ApplyVisibility();
            Apply(displayPercent);
        }

        private void Start()
        {
            TrySubscribe();
            ApplyVisibility();
        }

        private void TrySubscribe()
        {
            if (!subscribedOxygen && OxygenSystem.Instance != null)
            {
                OxygenSystem.Instance.OnO2Changed += HandleO2Changed;
                targetPercent = displayPercent = OxygenSystem.Instance.Percent;
                subscribedOxygen = true;
            }
            if (!subscribedGame && GameManager.Instance != null)
            {
                GameManager.Instance.OnRunStart += HandleRunStart;
                GameManager.Instance.OnRunEnd += HandleRunEnd;
                subscribedGame = true;
            }
            if (!subscribedWeight && WeightSystem.Instance != null)
            {
                WeightSystem.Instance.OnWeightChanged += HandleWeightChanged;
                ApplyWeight(WeightSystem.Instance.CurrentWeight);
                weightInitialized = true;
                subscribedWeight = true;
            }
        }

        private void HandleWeightChanged(float current)
        {
            ApplyWeight(current);
            if (weightInitialized) pulseTime = 0f;
            weightInitialized = true;
        }

        private void ApplyWeight(float current)
        {
            if (weightLabel == null) return;
            float max = WeightSystem.Instance != null ? WeightSystem.Instance.MaxWeight : 0f;
            weightLabel.text = string.Format(weightFormat, current, max);
            float t = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            weightLabelBaseColor = t < 0.5f
                ? Color.Lerp(weightColorLight, weightColorMid, t / 0.5f)
                : Color.Lerp(weightColorMid, weightColorMax, (t - 0.5f) / 0.5f);
            if (pulseTime < 0f) weightLabel.color = weightLabelBaseColor;
        }

        private void UpdatePulse()
        {
            if (pulseTime < 0f || weightLabel == null) return;
            pulseTime += Time.unscaledDeltaTime;
            float t = pulseTime / pulseDuration;
            if (t >= 1f)
            {
                weightLabel.transform.localScale = weightLabelBaseScale;
                weightLabel.color = weightLabelBaseColor;
                pulseTime = -1f;
                return;
            }
            float curve = pulseCurve.Evaluate(t);
            float scale = 1f + (pulseScale - 1f) * curve;
            weightLabel.transform.localScale = weightLabelBaseScale * scale;
            weightLabel.color = Color.Lerp(weightLabelBaseColor, pulseFlashColor, curve * pulseFlashAmount);
        }

        private void OnDestroy()
        {
            if (OxygenSystem.Instance != null) OxygenSystem.Instance.OnO2Changed -= HandleO2Changed;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRunStart -= HandleRunStart;
                GameManager.Instance.OnRunEnd -= HandleRunEnd;
            }
            if (WeightSystem.Instance != null) WeightSystem.Instance.OnWeightChanged -= HandleWeightChanged;
        }

        private void Update()
        {
            if (smoothing <= 0f) displayPercent = targetPercent;
            else displayPercent = Mathf.Lerp(displayPercent, targetPercent, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
            Apply(displayPercent);
            ApplyTimer();
            UpdatePulse();
        }

        private void ApplyTimer()
        {
            if (timerLabel == null) return;
            float seconds = GameManager.Instance != null ? GameManager.Instance.RunTimeSeconds : 0f;
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            timerLabel.text = $"{total / 60:00}:{total % 60:00}";
        }

        private void HandleO2Changed(float percent) => targetPercent = percent;
        private void HandleRunStart() => ApplyVisibility();
        private void HandleRunEnd(RunEndReason _) => ApplyVisibility();

        private void ApplyVisibility()
        {
            if (root == null) return;
            if (!hidePreRun) { root.SetActive(true); return; }
            bool show = GameManager.Instance != null && GameManager.Instance.State == RunState.Running;
            root.SetActive(show);
        }

        private void Apply(float percent)
        {
            float p = Mathf.Clamp01(percent);
            if (fillImage != null) fillImage.fillAmount = p;
            if (percentLabel != null) percentLabel.text = string.Format(percentFormat, p * 100f);
        }
    }
}
