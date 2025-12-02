using UnityEngine;

namespace Utilities
{
    public class VibrationManager : MonoBehaviour
    {
        private static VibrationManager _instance;
        public static VibrationManager Instance => _instance;

        private bool _vibrationEnabled = true;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved preference
            _vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        }

        public bool VibrationEnabled
        {
            get => _vibrationEnabled;
            set
            {
                _vibrationEnabled = value;
                PlayerPrefs.SetInt("VibrationEnabled", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public void VibrateLight()
        {
            if (!_vibrationEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        public void VibrateMedium()
        {
            if (!_vibrationEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                    {
                        vibrator.Call("vibrate", 100L);
                    }
                }
            }
#endif
        }

        public void VibrateHeavy()
        {
            if (!_vibrationEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                    {
                        vibrator.Call("vibrate", 200L);
                    }
                }
            }
#endif
        }
    }
}