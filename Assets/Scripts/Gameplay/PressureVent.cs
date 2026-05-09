using UnityEngine;

namespace Ballast.Gameplay
{
    public class PressureVent : MonoBehaviour
    {
        public enum VentState { Idle, Warning, Firing, Cooldown }

        [Header("Jet")]
        [Tooltip("Local-space direction the jet fires. Authored toward +Z (forward); spawner rotates Y by +90 (left wall) or -90 (right wall).")]
        [SerializeField] private Vector3 jetDirection = Vector3.forward;
        [SerializeField] private float impulseOnEnter = 12f;
        [SerializeField] private float continuousAcceleration = 40f;

        [Header("Cycle (seconds)")]
        [SerializeField, Min(0f)] private float idleDuration = 2f;
        [SerializeField, Min(0f)] private float warningDuration = 0.6f;
        [SerializeField, Min(0f)] private float firingDuration = 0.8f;
        [SerializeField, Min(0f)] private float cooldownDuration = 2f;
        [SerializeField, Min(0f)] private float startOffset = 0f;

        [Header("Telegraph (vent body)")]
        [SerializeField] private Renderer telegraphRenderer;
        [SerializeField] private Color idleColor = Color.gray;
        [SerializeField] private Color warningColor = new Color(1f, 0.7f, 0.1f);
        [SerializeField] private Color firingColor = new Color(1f, 0.2f, 0.2f);
        [Tooltip("One-shot played when entering Warning. Should not be set to loop.")]
        [SerializeField] private AudioSource warningAudio;
        [Tooltip("Looping audio while Firing. Set the AudioSource to loop; the script Plays/Stops it on state changes.")]
        [SerializeField] private AudioSource firingAudio;

        [Header("Jet effect")]
        [SerializeField] private ParticleSystem warningJet;
        [SerializeField] private ParticleSystem firingJet;

        private VentState state = VentState.Idle;
        private float stateTimer;
        private MaterialPropertyBlock mpb;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private DiverController diverInJet;
        private Rigidbody diverRb;

        private void Awake()
        {
            mpb = new MaterialPropertyBlock();
            stateTimer = startOffset;
            ApplyTelegraph(idleColor);
        }

        private void Update()
        {
            stateTimer += Time.deltaTime;
            switch (state)
            {
                case VentState.Idle:
                    if (stateTimer >= idleDuration) Transition(VentState.Warning);
                    break;
                case VentState.Warning:
                    if (stateTimer >= warningDuration) Transition(VentState.Firing);
                    break;
                case VentState.Firing:
                    if (stateTimer >= firingDuration) Transition(VentState.Cooldown);
                    break;
                case VentState.Cooldown:
                    if (stateTimer >= cooldownDuration) Transition(VentState.Idle);
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (state != VentState.Firing || diverRb == null) return;
            if (!TryGetJetWorldDir(out Vector3 dir)) return;
            diverRb.AddForce(dir * continuousAcceleration, ForceMode.Acceleration);
        }

        private bool TryGetJetWorldDir(out Vector3 dir)
        {
            dir = transform.TransformDirection(jetDirection);
            dir.y = 0f;
            dir.z = 0f;
            if (dir.sqrMagnitude < 0.0001f) return false;
            dir.Normalize();
            return true;
        }

        private void Transition(VentState next)
        {
            state = next;
            stateTimer = 0f;
            switch (next)
            {
                case VentState.Idle:
                case VentState.Cooldown:
                    ApplyTelegraph(idleColor);
                    StopJet(warningJet);
                    StopJet(firingJet);
                    if (firingAudio != null && firingAudio.isPlaying) firingAudio.Stop();
                    break;
                case VentState.Warning:
                    ApplyTelegraph(warningColor);
                    if (warningAudio != null) warningAudio.Play();
                    PlayJet(warningJet);
                    break;
                case VentState.Firing:
                    ApplyTelegraph(firingColor);
                    StopJet(warningJet);
                    PlayJet(firingJet);
                    if (firingAudio != null && !firingAudio.isPlaying) firingAudio.Play();
                    if (diverRb != null && TryGetJetWorldDir(out Vector3 fireDir))
                        diverRb.AddForce(fireDir * impulseOnEnter, ForceMode.Impulse);
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var diver = other.GetComponentInParent<DiverController>();
            if (diver == null) return;
            diverInJet = diver;
            diverRb = diver.GetComponent<Rigidbody>();

            if (state == VentState.Firing && diverRb != null && TryGetJetWorldDir(out Vector3 dir))
                diverRb.AddForce(dir * impulseOnEnter, ForceMode.Impulse);
        }

        private void OnTriggerExit(Collider other)
        {
            var diver = other.GetComponentInParent<DiverController>();
            if (diver == null || diver != diverInJet) return;
            diverInJet = null;
            diverRb = null;
        }

        private static void PlayJet(ParticleSystem ps)
        {
            if (ps == null) return;
            if (!ps.isPlaying) ps.Play(true);
        }

        private static void StopJet(ParticleSystem ps)
        {
            if (ps == null) return;
            if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void ApplyTelegraph(Color c)
        {
            if (telegraphRenderer == null) return;
            telegraphRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, c);
            mpb.SetColor(ColorId, c);
            telegraphRenderer.SetPropertyBlock(mpb);
        }
    }
}
