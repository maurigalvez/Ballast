using UnityEngine;

namespace Ballast.Gameplay
{
    [RequireComponent(typeof(DiverInventory))]
    public class ItemAudioCues : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip dropClip;
        [SerializeField, Range(0f, 1f)] private float dropVolume = 1f;

        private DiverInventory inventory;

        private void Reset() => audioSource = GetComponent<AudioSource>();

        private void Awake()
        {
            inventory = GetComponent<DiverInventory>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            inventory.OnDropped += HandleDropped;
        }

        private void OnDisable()
        {
            inventory.OnDropped -= HandleDropped;
        }

        private void HandleDropped(ItemPickup item)
        {
            if (audioSource != null && dropClip != null)
                audioSource.PlayOneShot(dropClip, dropVolume);
        }
    }
}
