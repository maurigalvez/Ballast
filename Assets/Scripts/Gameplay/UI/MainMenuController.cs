using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ballast.Gameplay
{
    public class MainMenuController : MonoBehaviour
    {
        private const string VolumePrefKey = "MasterVolume";

        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button backButton;
        [SerializeField] private CanvasGroup settingsPanel;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private float playFadeDuration = 0.5f;

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            if (backButton != null) backButton.onClick.AddListener(OnBack);

            float vol = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
            AudioListener.volume = vol;
            if (volumeSlider != null)
            {
                volumeSlider.SetValueWithoutNotify(vol);
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            SetPanel(false);
        }

        private void SetPanel(bool visible)
        {
            if (settingsPanel == null) return;
            settingsPanel.alpha = visible ? 1f : 0f;
            settingsPanel.interactable = visible;
            settingsPanel.blocksRaycasts = visible;
        }

        private void OnPlay()
        {
            if (playButton != null) playButton.interactable = false;
            if (settingsButton != null) settingsButton.interactable = false;

            var fader = ScreenFader.Instance;
            if (fader != null) StartCoroutine(PlayRoutine(fader));
            else SceneManager.LoadScene(gameSceneName);
        }

        private IEnumerator PlayRoutine(ScreenFader fader)
        {
            yield return fader.FadeTo(1f, playFadeDuration);
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnSettings() => SetPanel(true);
        private void OnBack() => SetPanel(false);

        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(VolumePrefKey, value);
        }

        private void OnFullscreenChanged(bool value)
        {
            Screen.fullScreen = value;
        }
    }
}
