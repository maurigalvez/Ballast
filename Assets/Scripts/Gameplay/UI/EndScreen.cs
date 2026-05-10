using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ballast.Gameplay
{
    public class EndScreen : MonoBehaviour
    {
        [SerializeField] private ManifestRegistry registry;
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform rowParent;
        [SerializeField] private ChecklistRow rowPrefab;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text timeLabel;
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private float fadeDuration = 0.5f;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (tryAgainButton != null) tryAgainButton.onClick.AddListener(OnTryAgain);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnRunEnd += HandleRunEnd;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnRunEnd -= HandleRunEnd;
        }

        private void HandleRunEnd(RunEndReason reason)
        {
            var fader = ScreenFader.Instance;
            if (fader != null) fader.StartCoroutine(ShowRoutine(reason, fader));
            else { BuildContent(reason); if (panel != null) panel.SetActive(true); }
        }

        private IEnumerator ShowRoutine(RunEndReason reason, ScreenFader fader)
        {
            yield return fader.FadeTo(1f, fadeDuration);
            BuildContent(reason);
            if (panel != null) panel.SetActive(true);
            yield return fader.FadeTo(0f, fadeDuration);
        }

        private void BuildContent(RunEndReason reason)
        {
            if (titleLabel != null) titleLabel.text = TitleFor(reason);
            if (timeLabel != null) timeLabel.text = FormatTime(GameManager.Instance != null ? GameManager.Instance.RunTimeSeconds : 0f);

            if (registry == null || rowPrefab == null || rowParent == null) return;
            for (int i = rowParent.childCount - 1; i >= 0; i--) Destroy(rowParent.GetChild(i).gameObject);

            var delivered = GameManager.Instance != null ? GameManager.Instance.DeliveredManifest : null;
            var items = registry.Items;
            for (int i = 0; i < items.Count; i++)
            {
                var data = items[i];
                if (data == null) continue;
                bool ticked = delivered != null && Contains(delivered, data);
                var row = Instantiate(rowPrefab, rowParent);
                row.Bind(data, ticked);
            }
        }

        private static bool Contains(System.Collections.Generic.IReadOnlyList<ItemData> list, ItemData target)
        {
            for (int i = 0; i < list.Count; i++) if (list[i] == target) return true;
            return false;
        }

        private static string TitleFor(RunEndReason reason)
        {
            switch (reason)
            {
                case RunEndReason.Success: return "ESCAPED";
                case RunEndReason.ManifestFailed: return "MANIFEST INCOMPLETE";
                case RunEndReason.CeilingCaught: return "CRUSHED";
                case RunEndReason.O2Empty:
                default: return "OUT OF OXYGEN";
            }
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int m = total / 60;
            int s = total % 60;
            return $"{m:00}:{s:00}";
        }

        private void OnTryAgain()
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }

        private void OnMainMenu()
        {
            Debug.Log("[EndScreen] Main menu not implemented; reloading scene.");
            OnTryAgain();
        }
    }
}
