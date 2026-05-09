using UnityEngine;

namespace Ballast.Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    public class O2AudioCues : MonoBehaviour
    {
        private enum BreathState { Idle, Warning, Critical, Empty }

        [SerializeField] private AudioSource breathingSource;
        [SerializeField] private AudioDistortionFilter distortionFilter;

        [SerializeField] private AudioClip[] idleClips;
        [SerializeField] private AudioClip[] warningClips;
        [SerializeField] private AudioClip[] criticalClips;

        [SerializeField, Range(0f, 1f)] private float idleVolume = 0.4f;
        [SerializeField, Range(0f, 1f)] private float warningVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float criticalVolume = 1f;
        [SerializeField] private float normalPitch = 1f;
        [SerializeField] private float criticalPitch = 0.9f;
        [SerializeField, Range(0f, 1f)] private float distortionLevel = 0.3f;

        [Header("Gap between breaths (seconds)")]
        [SerializeField] private Vector2 idleGap = new Vector2(3.5f, 5.5f);
        [SerializeField] private Vector2 warningGap = new Vector2(1.5f, 2.5f);
        [SerializeField] private Vector2 criticalGap = new Vector2(0.5f, 1.2f);

        private BreathState currentState = BreathState.Idle;
        private bool initialized;
        private float nextPlayTime;
        private int lastClipIndex = -1;

        private void Reset()
        {
            breathingSource = GetComponent<AudioSource>();
            distortionFilter = GetComponent<AudioDistortionFilter>();
        }

        private void Awake()
        {
            if (breathingSource == null) breathingSource = GetComponent<AudioSource>();
            if (distortionFilter == null) distortionFilter = GetComponent<AudioDistortionFilter>();

            if (breathingSource != null)
            {
                breathingSource.loop = false;
                breathingSource.playOnAwake = false;
            }
            if (distortionFilter != null)
            {
                distortionFilter.distortionLevel = distortionLevel;
                distortionFilter.enabled = false;
            }
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Start()
        {
            // Re-subscribe in case OxygenSystem.Instance wasn't ready in OnEnable.
            Unsubscribe();
            Subscribe();
            SyncToCurrentPercent();
            initialized = true;
        }

        private void Subscribe()
        {
            if (OxygenSystem.Instance == null) return;
            OxygenSystem.Instance.OnO2Warning += HandleWarning;
            OxygenSystem.Instance.OnO2Critical += HandleCritical;
            OxygenSystem.Instance.OnO2Empty += HandleEmpty;
        }

        private void Unsubscribe()
        {
            if (OxygenSystem.Instance == null) return;
            OxygenSystem.Instance.OnO2Warning -= HandleWarning;
            OxygenSystem.Instance.OnO2Critical -= HandleCritical;
            OxygenSystem.Instance.OnO2Empty -= HandleEmpty;
        }

        private void SyncToCurrentPercent()
        {
            var o2 = OxygenSystem.Instance;
            if (o2 == null)
            {
                EnterState(BreathState.Idle);
                return;
            }

            float p = o2.Percent;
            if (p <= 0f) EnterState(BreathState.Empty);
            else if (p <= o2.CriticalThreshold) EnterState(BreathState.Critical);
            else if (p <= o2.WarningThreshold) EnterState(BreathState.Warning);
            else EnterState(BreathState.Idle);
        }

        private void HandleWarning() => EnterState(BreathState.Warning);
        private void HandleCritical() => EnterState(BreathState.Critical);
        private void HandleEmpty() => EnterState(BreathState.Empty);

        private void Update()
        {
            if (currentState == BreathState.Empty) return;
            if (breathingSource == null) return;
            if (GameManager.Instance != null && GameManager.Instance.State != RunState.Running) return;
            if (breathingSource.isPlaying) return;
            if (Time.time < nextPlayTime) return;

            PlayCurrentState();
        }

        private void EnterState(BreathState next)
        {
            if (initialized && next == currentState) return;
            currentState = next;

            if (breathingSource == null) return;

            switch (next)
            {
                case BreathState.Idle:
                    SetDistortion(false);
                    breathingSource.pitch = normalPitch;
                    breathingSource.volume = idleVolume;
                    break;
                case BreathState.Warning:
                    SetDistortion(false);
                    breathingSource.pitch = normalPitch;
                    breathingSource.volume = warningVolume;
                    break;
                case BreathState.Critical:
                    SetDistortion(true);
                    breathingSource.pitch = criticalPitch;
                    breathingSource.volume = criticalVolume;
                    break;
                case BreathState.Empty:
                    SetDistortion(false);
                    breathingSource.pitch = normalPitch;
                    breathingSource.Stop();
                    return;
            }

            // Stop any in-flight clip from the previous state and play a fresh one immediately.
            breathingSource.Stop();
            if (GameManager.Instance != null && GameManager.Instance.State != RunState.Running) return;
            PlayCurrentState();
        }

        private void PlayCurrentState()
        {
            AudioClip[] pool = currentState switch
            {
                BreathState.Idle => idleClips,
                BreathState.Warning => warningClips,
                BreathState.Critical => criticalClips,
                _ => null
            };

            AudioClip pick = PickRandom(pool);
            if (pick == null) return;

            breathingSource.clip = pick;
            breathingSource.Play();

            Vector2 gap = currentState switch
            {
                BreathState.Warning => warningGap,
                BreathState.Critical => criticalGap,
                _ => idleGap
            };
            nextPlayTime = Time.time + pick.length + Random.Range(gap.x, gap.y);
        }

        private AudioClip PickRandom(AudioClip[] pool)
        {
            if (pool == null || pool.Length == 0) return null;
            if (pool.Length == 1) { lastClipIndex = 0; return pool[0]; }

            int idx = Random.Range(0, pool.Length);
            if (idx == lastClipIndex) idx = (idx + 1) % pool.Length;
            lastClipIndex = idx;
            return pool[idx];
        }

        private void SetDistortion(bool enabled)
        {
            if (distortionFilter == null) return;
            distortionFilter.distortionLevel = distortionLevel;
            distortionFilter.enabled = enabled;
        }
    }
}
