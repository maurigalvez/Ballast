using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Ballast.Gameplay
{
    public class WeightGate : MonoBehaviour
    {
        [Header("Gate")]
        [SerializeField] protected int maxWeight = 5;
        [SerializeField] protected LayerMask diverLayer;
        [SerializeField] protected Collider blockCollider;

        [Header("Visuals")]
        [SerializeField] protected Renderer ringRenderer;
        [SerializeField] protected Color allowColor = Color.yellow;
        [SerializeField] protected Color blockColor = Color.red;
        [SerializeField] protected TMP_Text label;

        [Header("Block response")]
        [SerializeField] protected float blockKnockback = 18f;
        [SerializeField] protected float redFlashDuration = 0.25f;

        [Header("Audio")]
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip openCue;

        public int MaxWeight => maxWeight;
        public bool IsOpen { get; protected set; }
        public event Action<WeightGate> OnApproach;

        protected bool approachFired;
        protected MaterialPropertyBlock mpb;
        protected static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        protected static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        protected virtual void Awake()
        {
            mpb = new MaterialPropertyBlock();
        }

        protected virtual void Start()
        {
            ApplyRingColor(allowColor);
            if (label != null) label.text = $"MAX {maxWeight}";
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (IsOpen) return;
            if (((1 << other.gameObject.layer) & diverLayer) == 0) return;
            HandleDiverEntered(other);
        }

        protected virtual void HandleDiverEntered(Collider diver)
        {
            float weight = WeightSystem.Instance != null ? WeightSystem.Instance.CurrentWeight : 0f;
            if (weight > maxWeight) Block(diver);
            else Open();
        }

        protected virtual void Block(Collider diver)
        {
            var rb = diver.attachedRigidbody;
            if (rb != null)
            {
                Vector3 push = transform.up;
                if (push.sqrMagnitude < 0.0001f) push = Vector3.up;
                rb.AddForce(push.normalized * blockKnockback, ForceMode.Impulse);
            }
            StartCoroutine(FlashRed());
        }

        protected virtual void Open()
        {
            IsOpen = true;
            if (blockCollider != null) blockCollider.enabled = false;
            ApplyRingColor(allowColor * 0.4f);
        }

        private IEnumerator FlashRed()
        {
            ApplyRingColor(blockColor);
            yield return new WaitForSeconds(redFlashDuration);
            if (!IsOpen) ApplyRingColor(allowColor);
        }

        private void ApplyRingColor(Color c)
        {
            if (ringRenderer == null) return;
            ringRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, c);
            mpb.SetColor(EmissionColorId, c);
            ringRenderer.SetPropertyBlock(mpb);
        }

        public void NotifyApproach()
        {
            if (approachFired) return;
            approachFired = true;
            OnApproach?.Invoke(this);
            GameManager.Instance?.NotifyGateApproach(this);
        }
    }
}
