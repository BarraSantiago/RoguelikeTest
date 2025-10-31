using UnityEngine;
using UnityEngine.UI;
using PlayerDirectory;
using PlayerDirectory.LevelingSystem;
    using TMPro;

namespace View.UI.Leveling
{
    public class LevelExperienceUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider xpSlider;
        [SerializeField] private Text xpText; 
        [SerializeField] private TextMeshProUGUI xpTextTMP; 
        [SerializeField] private Text levelText; 
        [SerializeField] private TextMeshProUGUI levelTextTMP; 

        [Header("Options")] 
        [SerializeField] private bool updateEveryFrame = true;
        [SerializeField, Range(0f, 1f)] private float smoothTime = 0.1f;

        private Player _player;
        private LevelSystem _levelSystem;
        private float _sliderVelocity;

        private void Awake()
        {
            _player = FindObjectOfType<Player>();
            if (_player) _levelSystem = _player.levelSystem;
            if (!xpSlider) return;
            xpSlider.minValue = 0f;
            if (_levelSystem != null) xpSlider.maxValue = Mathf.Max(1f, _levelSystem.ExperienceToNextLevel);
        }

        private void OnEnable()
        {
            if (!_player)
            {
                _player = FindObjectOfType<Player>();
                if (_player) _levelSystem = _player.levelSystem;
            }

            RefreshImmediate();
        }

        private void Update()
        {
            if (!updateEveryFrame) return;
            Refresh();
        }

        public void RefreshImmediate()
        {
            if (_levelSystem == null) return;
            if (xpSlider)
            {
                xpSlider.maxValue = Mathf.Max(1f, _levelSystem.ExperienceToNextLevel);
                xpSlider.value = _levelSystem.CurrentExperience;
            }

            UpdateTexts(_levelSystem.CurrentExperience, _levelSystem.ExperienceToNextLevel, _levelSystem.CurrentLevel);
        }

        public void Refresh()
        {
            if (_levelSystem == null) return;

            if (xpSlider) xpSlider.maxValue = Mathf.Max(1f, _levelSystem.ExperienceToNextLevel);

            if (xpSlider)
            {
                float target = Mathf.Clamp(_levelSystem.CurrentExperience, 0f, _levelSystem.ExperienceToNextLevel);
                if (smoothTime <= 0f)
                {
                    xpSlider.value = target;
                }
                else
                {
                    xpSlider.value = Mathf.SmoothDamp(xpSlider.value, target, ref _sliderVelocity, smoothTime);
                }
            }

            UpdateTexts(_levelSystem.CurrentExperience, _levelSystem.ExperienceToNextLevel, _levelSystem.CurrentLevel);
        }

        private void UpdateTexts(float current, float required, int level)
        {
            string xpStr = $"{current:0}/{required:0} ({(required > 0 ? (current / required * 100f) : 0f):0}%)";
            if (xpTextTMP) xpTextTMP.text = xpStr;
            if (xpText) xpText.text = xpStr;

            string lvlStr = $"Level {level}";
            if (levelTextTMP) levelTextTMP.text = lvlStr;
            if (levelText) levelText.text = lvlStr;
        }
    }
}