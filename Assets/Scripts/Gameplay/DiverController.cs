using UnityEngine;

namespace Ballast.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class DiverController : MonoBehaviour
    {
        [Header("Descent")]
        [SerializeField, Min(0f)] private float descentSpeed = 2f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void FixedUpdate()
        {
            // Constant descent — direct velocity write on Y only. Player cannot halt or reverse.
            Vector3 v = rb.linearVelocity;
            v.y = -descentSpeed;
            rb.linearVelocity = v;
        }
    }
}
