using UnityEngine;

namespace Ballast.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(InputReader))]
    public class DiverController : MonoBehaviour
    {
        [Header("Descent")]
        [SerializeField, Min(0f)] private float descentSpeed = 2f;

        [Header("Lateral steering")]
        [SerializeField] private float lateralForce = 30f;
        [SerializeField] private float maxLateralSpeed = 5f;

        [Header("Inertia")]
        [SerializeField, Range(0f, 10f)] private float linearDamping = 4f;

        [Header("Wall response")]
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private float wallKnockback = 8f;

        [Header("Weight coupling")]
        [SerializeField]
        private AnimationCurve weightMovementMultiplier = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(5f, 0.6f),
            new Keyframe(10f, 0.3f)
        );

        [SerializeField]
        private AnimationCurve weightDescentMultiplier = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(5f, 1.5f),
            new Keyframe(10f, 2.5f)
        );

        private Rigidbody rb;
        private InputReader input;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<InputReader>();

            rb.useGravity = false;
            rb.linearDamping = linearDamping;
            rb.angularDamping = 5f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void FixedUpdate()
        {
            float weight = WeightSystem.Instance != null ? WeightSystem.Instance.CurrentWeight : 0f;
            float mult = weightMovementMultiplier.Evaluate(weight);
            float descentMult = weightDescentMultiplier.Evaluate(weight);

            Vector3 v = rb.linearVelocity;
            v.y = -descentSpeed * descentMult;
            rb.linearVelocity = v;

            float inputX = input.Move.x;
            if (Mathf.Abs(inputX) > 0.001f)
            {
                rb.AddForce(Vector3.right * (inputX * lateralForce * mult), ForceMode.Force);
            }

            float cap = maxLateralSpeed * mult;
            v = rb.linearVelocity;
            if (Mathf.Abs(v.x) > cap)
            {
                v.x = Mathf.Sign(v.x) * cap;
                rb.linearVelocity = v;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & wallLayer) == 0) return;

            Vector3 n = collision.GetContact(0).normal;
            n.y = 0f;
            if (n.sqrMagnitude < 0.0001f) return;

            rb.AddForce(n.normalized * wallKnockback, ForceMode.Impulse);
        }
    }
}
