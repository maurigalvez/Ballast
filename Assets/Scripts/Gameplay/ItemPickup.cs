using System.Collections;
using UnityEngine;

namespace Ballast.Gameplay
{
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemData data;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private SphereCollider trigger;

        public ItemData Data => data;
        public MeshRenderer Renderer => meshRenderer;
        public SphereCollider Trigger => trigger;

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
            if (meshFilter != null && data.Mesh != null) meshFilter.sharedMesh = data.Mesh;
            if (meshRenderer != null)
            {
                var mat = meshRenderer.material;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor(EmissionColorId, data.EmissionColor);
            }
        }

        public void DisableTrigger()
        {
            if (trigger != null) trigger.enabled = false;
        }

        public IEnumerator DropAndFade(float upSpeed, float duration)
        {
            DisableTrigger();
            float t = 0f;
            Material mat = meshRenderer != null ? meshRenderer.material : null;
            Color baseColor = mat != null ? mat.color : Color.white;
            Color baseEmission = mat != null ? mat.GetColor(EmissionColorId) : Color.black;

            while (t < duration)
            {
                t += Time.deltaTime;
                transform.position += Vector3.up * (upSpeed * Time.deltaTime);
                if (mat != null)
                {
                    float a = Mathf.Lerp(1f, 0f, t / duration);
                    Color c = baseColor; c.a = a;
                    mat.color = c;
                    mat.SetColor(EmissionColorId, baseEmission * a);
                }
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
