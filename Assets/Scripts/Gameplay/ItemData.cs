using UnityEngine;

namespace Ballast.Gameplay
{
    [CreateAssetMenu(menuName = "Ballast/Item Data", fileName = "ItemData")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string itemName;
        [SerializeField, Min(0)] private int weight = 1;
        [SerializeField] private bool isManifestItem;
        [SerializeField] private Mesh mesh;
        [SerializeField, ColorUsage(true, true)] private Color emissionColor = Color.white;
        [SerializeField] private ItemPickup prefab;

        public string ItemName => itemName;
        public int Weight => weight;
        public bool IsManifestItem => isManifestItem;
        public Mesh Mesh => mesh;
        public Color EmissionColor => emissionColor;
        public ItemPickup Prefab => prefab;
    }
}
