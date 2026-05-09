using UnityEngine;

namespace Ballast.Gameplay
{
    /// Marker: this obstacle must spawn flush to the left or right tunnel wall, facing inward.
    public class WallMounted : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float wallInset = 0f;

        public float WallInset => wallInset;
    }
}
