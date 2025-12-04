using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace View.UI.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button openButton;
        [SerializeField] private TextMeshProUGUI pageIndicator;

        [Header("Tutorial Highlights")]
        [SerializeField] private GameObject joystickHighlight;
        [SerializeField] private GameObject levelUpHighlight;
        [SerializeField] private GameObject enemiesHighlight;

        private const string TutorialSeenKey = "TutorialSeen";
        private int _currentPage;
        private float _previousTimeScale;
        private bool _isOpen;

        private readonly TutorialPage[] _pages = new[]
        {
            new TutorialPage
            {
                Title = "Welcome!",
                Description = "This tutorial will teach you the basics of survival. Use the Next button to continue."
            },
            new TutorialPage
            {
                Title = "Movement",
                Description = "Use the joystick on the left side of the screen to move your character.\\n Your character will automatically attack nearby enemies.",
                HighlightType = HighlightType.Joystick
            },
            new TutorialPage
            {
                Title = "Combat",
                Description = "Enemies will spawn around you and try to attack. \\nDefeat them to gain experience and coins. Stay alert!",
                HighlightType = HighlightType.Enemies
            },
            new TutorialPage
            {
                Title = "Leveling Up",
                Description = "Gain enough experience to level up!\\nUpgrades will make your character stronger.\\nYou can reroll, skip, or banish options.",
                HighlightType = HighlightType.LevelUp
            },
            new TutorialPage
            {
                Title = "Survival",
                Description = "Your goal is to survive as long as possible. Collect coins, level up, and defeat enemies. Good luck!"
            }
        };

        private void Awake()
        {
            if (tutorialPanel)
                tutorialPanel.SetActive(false);

            nextButton?.onClick.AddListener(NextPage);
            previousButton?.onClick.AddListener(PreviousPage);
            closeButton?.onClick.AddListener(CloseTutorial);
            openButton?.onClick.AddListener(OpenTutorial);
        }

        private void Start()
        {
            bool hasSeenTutorial = PlayerPrefs.GetInt(TutorialSeenKey, 0) == 1;

            if (!hasSeenTutorial)
            {
                OpenTutorial();
            }
        }

        public void OpenTutorial()
        {
            if (_isOpen) return;

            _isOpen = true;
            _currentPage = 0;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            if (tutorialPanel)
                tutorialPanel.SetActive(true);

            UpdatePage();
        }

        public void CloseTutorial()
        {
            if (!_isOpen) return;

            _isOpen = false;
            Time.timeScale = _previousTimeScale;

            if (tutorialPanel)
                tutorialPanel.SetActive(false);

            HideAllHighlights();
            PlayerPrefs.SetInt(TutorialSeenKey, 1);
            PlayerPrefs.Save();
        }

        private void NextPage()
        {
            if (_currentPage < _pages.Length - 1)
            {
                _currentPage++;
                UpdatePage();
            }
            else
            {
                CloseTutorial();
            }
        }

        private void PreviousPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                UpdatePage();
            }
        }

        private void UpdatePage()
        {
            TutorialPage page = _pages[_currentPage];

            if (titleText)
                titleText.text = page.Title;

            if (descriptionText)
                descriptionText.text = page.Description;

            if (pageIndicator)
                pageIndicator.text = $"{_currentPage + 1}/{_pages.Length}";

            if (previousButton)
                previousButton.interactable = _currentPage > 0;

            if (nextButton)
            {
                TextMeshProUGUI buttonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText)
                    buttonText.text = _currentPage < _pages.Length - 1 ? "Next" : "Finish";
            }

            UpdateHighlights(page.HighlightType);
        }

        private void UpdateHighlights(HighlightType type)
        {
            HideAllHighlights();

            switch (type)
            {
                case HighlightType.Joystick:
                    if (joystickHighlight)
                        joystickHighlight.SetActive(true);
                    break;
                case HighlightType.LevelUp:
                    if (levelUpHighlight)
                        levelUpHighlight.SetActive(true);
                    break;
                case HighlightType.Enemies:
                    if (enemiesHighlight)
                        enemiesHighlight.SetActive(true);
                    break;
            }
        }

        private void HideAllHighlights()
        {
            if (joystickHighlight)
                joystickHighlight.SetActive(false);
            if (levelUpHighlight)
                levelUpHighlight.SetActive(false);
            if (enemiesHighlight)
                enemiesHighlight.SetActive(false);
        }

        public void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(TutorialSeenKey);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            nextButton?.onClick.RemoveListener(NextPage);
            previousButton?.onClick.RemoveListener(PreviousPage);
            closeButton?.onClick.RemoveListener(CloseTutorial);
        }

        private struct TutorialPage
        {
            public string Title;
            public string Description;
            public HighlightType HighlightType;
        }

        private enum HighlightType
        {
            None,
            Joystick,
            LevelUp,
            Enemies
        }
    }
}