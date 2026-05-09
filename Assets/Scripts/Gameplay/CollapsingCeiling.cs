using UnityEngine;

namespace Ballast.Gameplay
{
    public class CollapsingCeiling : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float descentSpeed = 0.4f;
        [SerializeField] private LayerMask diverLayer;

        private bool stopped;

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnRunEnd += _ => stopped = true;
        }

        private void Update()
        {
            if (stopped) return;
            if (GameManager.Instance != null && GameManager.Instance.State != RunState.Running) return;
            transform.position += Vector3.down * (descentSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (stopped) return;
            if (((1 << other.gameObject.layer) & diverLayer) == 0) return;
            stopped = true;
            GameManager.Instance?.RunEnd(RunEndReason.CeilingCaught);
        }
    }
}
