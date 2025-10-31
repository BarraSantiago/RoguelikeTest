using UnityEngine;

namespace View.UI.Menus
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        private bool _musicEnabled = true;
        private bool _sfxEnabled = true;

        public bool MusicEnabled
        {
            get => _musicEnabled;
            set
            {
                _musicEnabled = value;
                if (musicSource)
                {
                    musicSource.mute = !value;
                }
                PlayerPrefs.SetInt("MusicEnabled", value ? 1 : 0);
            }
        }

        public bool SFXEnabled
        {
            get => _sfxEnabled;
            set
            {
                _sfxEnabled = value;
                if (sfxSource)
                {
                    sfxSource.mute = !value;
                }
                PlayerPrefs.SetInt("SFXEnabled", value ? 1 : 0);
            }
        }

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }

        private void LoadSettings()
        {
            _musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            _sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

            if (musicSource)
            {
                musicSource.mute = !_musicEnabled;
            }

            if (sfxSource)
            {
                sfxSource.mute = !_sfxEnabled;
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource && clip && _sfxEnabled)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }
}