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

        private Rigidbody rb;
        private InputReader input;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<InputReader>();

            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void FixedUpdate()
        {
            Vector3 v = rb.linearVelocity;
            v.y = -descentSpeed;
            rb.linearVelocity = v;

            float inputX = input.Move.x;
            if (Mathf.Abs(inputX) > 0.001f)
            {
                rb.AddForce(Vector3.right * (inputX * lateralForce), ForceMode.Force);
            }

            v = rb.linearVelocity;
            if (Mathf.Abs(v.x) > maxLateralSpeed)
            {
                v.x = Mathf.Sign(v.x) * maxLateralSpeed;
                rb.linearVelocity = v;
            }
        }
    }
}
