using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Menus
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private PauseManager pauseManager;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;

        private void Start()
        {
            if (settingsPanel)
            {
                settingsPanel.SetActive(false);
            }

            if (musicToggle && AudioManager.Instance)
            {
                musicToggle.isOn = AudioManager.Instance.MusicEnabled;
                musicToggle.onValueChanged.AddListener(OnMusicToggled);
            }

            if (sfxToggle && AudioManager.Instance)
            {
                sfxToggle.isOn = AudioManager.Instance.SFXEnabled;
                sfxToggle.onValueChanged.AddListener(OnSFXToggled);
            }
        }

        public void OnResumeClicked()
        {
            pauseManager?.Resume();
        }

        public void OnSettingsClicked()
        {
            mainPanel?.SetActive(false);
            settingsPanel?.SetActive(true);
        }

        public void OnBackFromSettings()
        {
            settingsPanel?.SetActive(false);
            mainPanel?.SetActive(true);
        }

        public void OnMenuClicked()
        {
            pauseManager?.ChangeScene();
        }

        public void OnQuitClicked()
        {
            pauseManager?.QuitGame();
        }

        private void OnMusicToggled(bool value)
        {
            if (AudioManager.Instance)
            {
                AudioManager.Instance.MusicEnabled = value;
            }
        }

        private void OnSFXToggled(bool value)
        {
            if (AudioManager.Instance)
            {
                AudioManager.Instance.SFXEnabled = value;
            }
        }

        private void OnDestroy()
        {
            musicToggle?.onValueChanged.RemoveListener(OnMusicToggled);
            sfxToggle?.onValueChanged.RemoveListener(OnSFXToggled);
        }
    }
}