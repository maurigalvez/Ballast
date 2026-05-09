using UnityEngine;

namespace Ballast.Gameplay
{
    public class WeightGateProximity : MonoBehaviour
    {
        [SerializeField] private WeightGate gate;
        [SerializeField] private LayerMask diverLayer;

        private void Reset()
        {
            gate = GetComponentInParent<WeightGate>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (gate == null) return;
            if (((1 << other.gameObject.layer) & diverLayer) == 0) return;
            gate.NotifyApproach();
        }
    }
}
