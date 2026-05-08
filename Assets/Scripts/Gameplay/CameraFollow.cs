using UnityEngine;

namespace Ballast.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 3f, -5f);

        [Header("Smoothing")]
        [SerializeField, Range(0.05f, 1f)] private float horizontalSmoothTime = 0.2f;
        [Tooltip("Higher = camera floats down more slowly behind the diver.")]
        [SerializeField, Range(0.1f, 3f)] private float verticalSmoothTime = 0.8f;
        [SerializeField, Min(1f)] private float criticalDistanceBoost = 1.5f;

        private Vector3 horizontalVel;
        private float verticalVel;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredOffset = offset;
            if (WeightSystem.Instance != null && WeightSystem.Instance.Zone == WeightZone.Critical)
            {
                desiredOffset.z *= criticalDistanceBoost;
            }

            Vector3 desiredPos = target.position + desiredOffset;
            Vector3 current = transform.position;

            // Horizontal (X/Z) — responsive follow.
            Vector3 horizontalCurrent = new Vector3(current.x, 0f, current.z);
            Vector3 horizontalTarget = new Vector3(desiredPos.x, 0f, desiredPos.z);
            Vector3 horizontalNext = Vector3.SmoothDamp(horizontalCurrent, horizontalTarget, ref horizontalVel, horizontalSmoothTime);

            // Vertical (Y) — slower drift so the camera floats down behind the diver.
            float yNext = Mathf.SmoothDamp(current.y, desiredPos.y, ref verticalVel, verticalSmoothTime);

            transform.position = new Vector3(horizontalNext.x, yNext, horizontalNext.z);
            transform.LookAt(target);
        }
    }
}
