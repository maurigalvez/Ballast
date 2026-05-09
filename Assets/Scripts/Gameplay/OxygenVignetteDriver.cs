using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Ballast.Gameplay
{
    public class OxygenVignetteDriver : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [SerializeField, Range(0f, 1f)] private float startPercent = 0.25f;
        [SerializeField, Range(0f, 1f)] private float fullPercent = 0.10f;
        [SerializeField, Range(0f, 1f)] private float maxIntensity = 0.5f;
        [SerializeField] private float lerpSpeed = 4f;

        private Vignette vignette;
        private float targetIntensity;

        private void Awake()
        {
            if (volume == null) volume = GetComponent<Volume>();
            if (volume != null && volume.profile != null)
                volume.profile.TryGet(out vignette);
        }

        private void OnEnable()
        {
            if (OxygenSystem.Instance != null)
            {
                OxygenSystem.Instance.OnO2Changed += HandleO2Changed;
                HandleO2Changed(OxygenSystem.Instance.Percent);
            }
        }

        private void Start()
        {
            if (OxygenSystem.Instance != null)
            {
                OxygenSystem.Instance.OnO2Changed -= HandleO2Changed;
                OxygenSystem.Instance.OnO2Changed += HandleO2Changed;
                HandleO2Changed(OxygenSystem.Instance.Percent);
            }
        }

        private void OnDisable()
        {
            if (OxygenSystem.Instance != null)
                OxygenSystem.Instance.OnO2Changed -= HandleO2Changed;
        }

        private void HandleO2Changed(float percent)
        {
            if (percent >= startPercent) targetIntensity = 0f;
            else if (percent <= fullPercent) targetIntensity = maxIntensity;
            else
            {
                float t = Mathf.InverseLerp(startPercent, fullPercent, percent);
                targetIntensity = Mathf.Lerp(0f, maxIntensity, t);
            }
        }

        private void Update()
        {
            if (vignette == null) return;
            float current = vignette.intensity.value;
            float next = Mathf.Lerp(current, targetIntensity, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime));
            vignette.intensity.Override(next);
        }
    }
}
