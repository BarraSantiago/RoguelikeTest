using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace View.UI.Menus
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private PauseManager pauseManager;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private TMP_Dropdown performanceDropdown;

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

            if (vibrationToggle && VibrationManager.Instance)
            {
                vibrationToggle.isOn = VibrationManager.Instance.VibrationEnabled;
                vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);
            }

            if (performanceDropdown && PerformanceManager.Instance)
            {
                performanceDropdown.options.Clear();
                performanceDropdown.options.Add(new TMP_Dropdown.OptionData("Efficiency"));
                performanceDropdown.options.Add(new TMP_Dropdown.OptionData("Balanced"));
                performanceDropdown.options.Add(new TMP_Dropdown.OptionData("Ultra"));
                performanceDropdown.value = (int)PerformanceManager.Instance.CurrentQuality;
                performanceDropdown.onValueChanged.AddListener(OnPerformanceChanged);
            }

            if(_resumeButton)
            {
                _resumeButton.onClick.AddListener(OnResumeClicked);
            }
        }

        private void OnEnable()
        {
            Time.timeScale = 0f;
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            musicToggle?.onValueChanged.RemoveListener(OnMusicToggled);
            sfxToggle?.onValueChanged.RemoveListener(OnSFXToggled);
            vibrationToggle?.onValueChanged.RemoveListener(OnVibrationToggled);
            performanceDropdown?.onValueChanged.RemoveListener(OnPerformanceChanged);
        }

        public void OnResumeClicked()
        {
            pauseManager?.Resume();
        }

        private void OnVibrationToggled(bool value)
        {
            if (VibrationManager.Instance)
            {
                VibrationManager.Instance.VibrationEnabled = value;
            }
        }

        private void OnPerformanceChanged(int value)
        {
            if (PerformanceManager.Instance)
            {
                PerformanceManager.Instance.SetQualityLevel((PerformanceQuality)value);
            }
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
    }
}