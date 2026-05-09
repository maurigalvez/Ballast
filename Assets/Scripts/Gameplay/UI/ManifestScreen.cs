using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ballast.Gameplay
{
    public class ManifestScreen : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ManifestRegistry registry;
        [SerializeField] private Transform rowParent;
        [SerializeField] private ChecklistRow rowPrefab;
        [SerializeField] private Button startButton;
        [SerializeField] private GameObject panel;
        [SerializeField] private float fadeDuration = 0.5f;

        private bool starting;

        private void Awake()
        {
            BuildRows();
            if (startButton != null) startButton.onClick.AddListener(BeginRun);
        }

        private void Start()
        {
            if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn(fadeDuration);
        }

        private void BuildRows()
        {
            if (registry == null || rowPrefab == null || rowParent == null) return;
            for (int i = rowParent.childCount - 1; i >= 0; i--) Destroy(rowParent.GetChild(i).gameObject);
            var items = registry.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null) continue;
                var row = Instantiate(rowPrefab, rowParent);
                row.Bind(items[i].ItemName, false);
            }
        }

        public void OnPointerClick(PointerEventData eventData) => BeginRun();

        public void BeginRun()
        {
            if (starting) return;
            starting = true;

            var fader = ScreenFader.Instance;
            if (fader != null)
            {
                fader.StartCoroutine(BeginRunRoutine(fader));
            }
            else
            {
                HidePanel();
                if (GameManager.Instance != null) GameManager.Instance.StartRun();
            }
        }

        private IEnumerator BeginRunRoutine(ScreenFader fader)
        {
            yield return fader.FadeTo(1f, fadeDuration);
            HidePanel();
            yield return fader.FadeTo(0f, fadeDuration);
            if (GameManager.Instance != null) GameManager.Instance.StartRun();
        }

        private void HidePanel()
        {
            if (panel != null) panel.SetActive(false);
            else gameObject.SetActive(false);
        }
    }
}
