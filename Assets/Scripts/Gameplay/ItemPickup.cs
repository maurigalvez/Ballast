using UnityEngine;

namespace Ballast.Gameplay
{
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemData data;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Collider trigger;
        [SerializeField] private AudioClip pickupClip;
        [SerializeField] private AudioClip keyPickupClip;
        [SerializeField, Range(0f, 1f)] private float pickupVolume = 1f;

        public ItemData Data => data;
        public MeshRenderer Renderer => meshRenderer;
        public Collider Trigger => trigger;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        public void Bind(ItemData newData)
        {
            data = newData;
            ApplyData();
        }

        private void Awake()
        {
            ApplyData();
        }

        private void ApplyData()
        {
            if (data == null) return;
            if (data.Prefab != null) return;
            if (meshFilter != null && data.Mesh != null) meshFilter.sharedMesh = data.Mesh;
            if (meshRenderer != null)
            {
                var mat = meshRenderer.material;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor(EmissionColorId, data.EmissionColor);
            }
        }

        public void PlayPickupSfx()
        {
            AudioClip clip = (data != null && data.IsManifestItem && keyPickupClip != null)
                ? keyPickupClip
                : pickupClip;
            if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.position, pickupVolume);
        }

        public void DisableTrigger()
        {
            if (trigger != null) trigger.enabled = false;
        }
    }
}
