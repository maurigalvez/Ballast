#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ballast.Gameplay
{
    public class DebugWeightControl : MonoBehaviour
    {
        [SerializeField] private float step = 1f;

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || WeightSystem.Instance == null) return;

            if (kb.leftBracketKey.wasPressedThisFrame)
            {
                WeightSystem.Instance.RemoveWeight(step);
                Log();
            }
            else if (kb.rightBracketKey.wasPressedThisFrame)
            {
                WeightSystem.Instance.AddWeight(step);
                Log();
            }
        }

        private static void Log()
        {
            var ws = WeightSystem.Instance;
            Debug.Log($"[Weight] {ws.CurrentWeight:0.0}/{ws.MaxWeight:0.0} ({ws.Zone})");
        }
    }
}
#endif
