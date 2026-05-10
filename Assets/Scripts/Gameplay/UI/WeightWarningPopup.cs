using System.Collections;
using TMPro;
using UnityEngine;

namespace Ballast.Gameplay
{
    public class WeightWarningPopup : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;
        [SerializeField] private string message = "You are too heavy! Drop weight!";
        [SerializeField, Min(0.1f)] private float duration = 2.5f;

        private Coroutine routine;
        private bool subscribed;

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            TrySubscribe();
        }

        private void Start()
        {
            TrySubscribe();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnWeightGateBlocked -= HandleWarning;
        }

        private void TrySubscribe()
        {
            if (subscribed || GameManager.Instance == null) return;
            GameManager.Instance.OnWeightGateBlocked += HandleWarning;
            subscribed = true;
        }

        private void HandleWarning(WeightGate gate)
        {
            if (label != null) label.text = message;
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            if (root != null) root.SetActive(true);
            yield return new WaitForSeconds(duration);
            if (root != null) root.SetActive(false);
            routine = null;
        }
    }
}
