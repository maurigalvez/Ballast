using UnityEngine;

namespace Ballast.Gameplay
{
    public class RockSpur : MonoBehaviour
    {
        [SerializeField] private float knockback = 10f;
        [SerializeField] private float o2Penalty = 12f;
        [SerializeField] private float penaltyCooldown = 0.5f;
        private float lastPenaltyTime = -10f;

        private void OnCollisionEnter(Collision collision)
        {
            var rb = collision.rigidbody;
            if (rb == null) return;
            if (rb.GetComponent<DiverController>() == null) return;

            Vector3 dir = rb.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            rb.AddForce(dir.normalized * knockback, ForceMode.Impulse);

            if (Time.time - lastPenaltyTime >= penaltyCooldown)
            {
                OxygenSystem.Instance?.TakeDamage(o2Penalty);
                lastPenaltyTime = Time.time;
            }
        }
    }
}
