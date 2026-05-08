using UnityEngine;
using UnityEngine.InputSystem;

namespace Ballast.Gameplay
{
    public class InputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private bool useTilt = false;
        [SerializeField] private float tiltSensitivity = 2f;
        [SerializeField] private float tiltDeadzone = 0.05f;

        public Vector2 Move { get; private set; }

        private void OnEnable()
        {
            if (moveAction != null) moveAction.action.Enable();
        }

        private void OnDisable()
        {
            if (moveAction != null) moveAction.action.Disable();
        }

        private void Update()
        {
            Vector2 v = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

            if (useTilt && Accelerometer.current != null)
            {
                float tiltX = Accelerometer.current.acceleration.ReadValue().x * tiltSensitivity;
                if (Mathf.Abs(tiltX) > tiltDeadzone)
                    v.x = Mathf.Clamp(tiltX, -1f, 1f);
            }

            Move = v;
        }
    }
}
