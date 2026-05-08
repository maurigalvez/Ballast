using UnityEngine;

namespace Ballast.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 3f, -5f);
        [SerializeField, Range(0.05f, 1f)] private float smoothTime = 0.2f;
        [SerializeField, Min(1f)] private float criticalDistanceBoost = 1.5f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredOffset = offset;
            if (WeightSystem.Instance != null && WeightSystem.Instance.Zone == WeightZone.Critical)
            {
                desiredOffset.z *= criticalDistanceBoost;
            }

            Vector3 desiredPos = target.position + desiredOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
            transform.LookAt(target);
        }
    }
}
