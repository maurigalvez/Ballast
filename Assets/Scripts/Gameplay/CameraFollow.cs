using UnityEngine;

namespace Ballast.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float yOffset = 3f;
        [Tooltip("Higher = camera floats down more slowly behind the diver.")]
        [SerializeField, Range(0.1f, 3f)] private float verticalSmoothTime = 0.8f;

        private float verticalVel;

        private void LateUpdate()
        {
            if (target == null) return;

            float desiredY = target.position.y + yOffset;
            float yNext = Mathf.SmoothDamp(transform.position.y, desiredY, ref verticalVel, verticalSmoothTime);

            Vector3 p = transform.position;
            p.y = yNext;
            transform.position = p;
        }
    }
}
