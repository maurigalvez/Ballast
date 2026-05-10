using System.Collections;
using UnityEngine;

namespace Ballast.Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    public class EndAudioCues : MonoBehaviour
    {
        [SerializeField] private AudioSource cueSource;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip gameOverClip;

        [Header("Ambience Ducking")]
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField, Range(0f, 1f)] private float duckedVolume = 0.2f;
        [SerializeField] private float duckFadeDuration = 0.4f;

        private float originalAmbienceVolume;
        private bool ambienceVolumeCached;
        private Coroutine duckRoutine;

        private void Awake()
        {
            if (cueSource == null) cueSource = GetComponent<AudioSource>();
            cueSource.playOnAwake = false;

            if (ambienceSource != null)
            {
                originalAmbienceVolume = ambienceSource.volume;
                ambienceVolumeCached = true;
            }
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnRunEnd += HandleRunEnd;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnRunEnd -= HandleRunEnd;
        }

        private void HandleRunEnd(RunEndReason reason)
        {
            var clip = reason == RunEndReason.Success ? winClip : gameOverClip;
            if (clip == null || cueSource == null) return;

            if (duckRoutine != null) StopCoroutine(duckRoutine);
            duckRoutine = StartCoroutine(PlayWithDuck(clip));
        }

        private IEnumerator PlayWithDuck(AudioClip clip)
        {
            if (ambienceSource != null)
            {
                if (!ambienceVolumeCached)
                {
                    originalAmbienceVolume = ambienceSource.volume;
                    ambienceVolumeCached = true;
                }
                yield return FadeAmbience(ambienceSource.volume, duckedVolume, duckFadeDuration);
            }

            cueSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);

            if (ambienceSource != null)
            {
                yield return FadeAmbience(ambienceSource.volume, originalAmbienceVolume, duckFadeDuration);
            }

            duckRoutine = null;
        }

        private IEnumerator FadeAmbience(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                ambienceSource.volume = to;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                ambienceSource.volume = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            ambienceSource.volume = to;
        }
    }
}
