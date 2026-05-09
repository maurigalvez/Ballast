using System.Collections.Generic;
using UnityEngine;

namespace Ballast.Gameplay
{
    [CreateAssetMenu(menuName = "Ballast/Manifest Registry", fileName = "ManifestRegistry")]
    public class ManifestRegistry : ScriptableObject
    {
        [SerializeField] private ItemData[] manifestItems;

        public IReadOnlyList<ItemData> Items => manifestItems;
    }
}
