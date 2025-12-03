using UnityEngine;

namespace Utilities
{
    public enum PerformanceQuality
    {
        Efficiency = 0,
        Balanced = 1,
        Ultra = 2
    }

    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }

        public PerformanceQuality CurrentQuality { get; private set; } = PerformanceQuality.Balanced;

        private const string QualityKey = "PerformanceQuality";
        private float[] _fpsHistory = new float[60];
        private int _fpsHistoryIndex = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
        }

        private void Update()
        {
            _fpsHistory[_fpsHistoryIndex] = 1f / Time.unscaledDeltaTime;
            _fpsHistoryIndex = (_fpsHistoryIndex + 1) % _fpsHistory.Length;
        }

        public void SetQualityLevel(PerformanceQuality quality)
        {
            CurrentQuality = quality;
            ApplyQualitySettings();
            SaveSettings();
        }

        private void ApplyQualitySettings()
        {
            switch (CurrentQuality)
            {
                case PerformanceQuality.Efficiency:
                    QualitySettings.globalTextureMipmapLimit = 2;
                    Application.targetFrameRate = 40;
                    break;
                case PerformanceQuality.Balanced:
                    QualitySettings.globalTextureMipmapLimit = 1;
                    Application.targetFrameRate = 60;
                    break;
                case PerformanceQuality.Ultra:
                    QualitySettings.globalTextureMipmapLimit = 0;
                    Application.targetFrameRate = 120;
                    break;
            }
        }

        public float GetUpdateInterval()
        {
            float avgFps = GetAverageFPS();

            return CurrentQuality switch
            {
                PerformanceQuality.Efficiency => avgFps < 30 ? 0.2f : 0.1f,
                PerformanceQuality.Balanced => avgFps < 45 ? 0.1f : 0.05f,
                PerformanceQuality.Ultra => avgFps < 90 ? 0.05f : 0.015f,
                _ => 0.1f
            };
        }

        private float GetAverageFPS()
        {
            float sum = 0;
            foreach (float fps in _fpsHistory)
            {
                sum += fps;
            }
            return sum / _fpsHistory.Length;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetInt(QualityKey, (int)CurrentQuality);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            CurrentQuality = (PerformanceQuality)PlayerPrefs.GetInt(QualityKey, (int)PerformanceQuality.Balanced);
            ApplyQualitySettings();
        }
    }
}