using UnityEngine;

namespace Ballast.Gameplay
{
    public class SeaweedCurtain : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 1f)] private float slowMultiplier = 0.4f;

        private void OnTriggerEnter(Collider other)
        {
            var diver = other.GetComponentInParent<DiverController>();
            if (diver == null) return;
            diver.AddSlowSource(this, slowMultiplier);
        }

        private void OnTriggerExit(Collider other)
        {
            var diver = other.GetComponentInParent<DiverController>();
            if (diver == null) return;
            diver.RemoveSlowSource(this);
        }

        private void OnDisable()
        {
            var divers = FindObjectsByType<DiverController>(FindObjectsSortMode.None);
            foreach (var d in divers) d.RemoveSlowSource(this);
        }
    }
}
