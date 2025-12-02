using System.Collections.Generic;
using PlayerDirectory;
using PlayerDirectory.LevelingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Leveling
{
    public class LevelUpUI : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceCardPrefab;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button banishButton;
        [SerializeField] private Button rerollButton;
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipText;

        [Header("Charge Display")] 
        [SerializeField] private TextMeshProUGUI rerollChargesText;
        [SerializeField] private TextMeshProUGUI skipChargesText;
        [SerializeField] private TextMeshProUGUI banishChargesText;
        [SerializeField] private GameObject chargesPanel;

        [Header("Level Info")] 
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI choosePromptText;

        private Player _player;
        private LevelUpManager _levelUpManager;
        private List<ChoiceCard> _activeCards = new List<ChoiceCard>();
        private List<object> _currentChoices;
        private int _rerollCharges;
        private int _skipCharges;
        private int _banishCharges;
        private object _selectedForBanish;

        private void Awake()
        {
            skipButton.onClick.AddListener(OnSkipClicked);
            banishButton.onClick.AddListener(OnBanishClicked);
            rerollButton.onClick.AddListener(OnRerollClicked);

            tooltipPanel.SetActive(false);
            levelUpPanel.SetActive(false);
        }

        public void ShowLevelUpChoices(Player player, List<object> choices, int rerolls, int skips, int banishes)
        {
            _player = player;
            _currentChoices = choices;
            _rerollCharges = rerolls;
            _skipCharges = skips;
            _banishCharges = banishes;
            _levelUpManager = FindFirstObjectByType<LevelUpManager>();
            _selectedForBanish = null;

            ClearChoices();
            CreateChoiceCards(choices);
            UpdateUI();

            levelUpPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        private void UpdateUI()
        {
            // Update level display
            if (levelText != null)
            {
                levelText.text = $"Level {_player.levelSystem.CurrentLevel}";
            }

            // Update prompt text
            if (choosePromptText != null)
            {
                choosePromptText.text = _banishCharges > 0 && _selectedForBanish == null
                    ? "Select an option to banish"
                    : "Choose your upgrade";
            }

            // Update button states and text
            UpdateButtonStates();
            UpdateChargeDisplays();
        }

        private void UpdateButtonStates()
        {
            // Reroll button
            rerollButton.interactable = _rerollCharges > 0;
            ColorBlock rerollColors = rerollButton.colors;
            rerollColors.normalColor = _rerollCharges > 0 ? Color.white : Color.gray;
            rerollButton.colors = rerollColors;

            // Skip button
            skipButton.interactable = _skipCharges > 0;
            ColorBlock skipColors = skipButton.colors;
            skipColors.normalColor = _skipCharges > 0 ? Color.white : Color.gray;
            skipButton.colors = skipColors;

            // Banish button
            banishButton.interactable = _banishCharges > 0;
            ColorBlock banishColors = banishButton.colors;
            banishColors.normalColor = _banishCharges > 0 ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            banishButton.colors = banishColors;

            // Update banish button text
            TextMeshProUGUI banishText = banishButton.GetComponentInChildren<TextMeshProUGUI>();
            if (banishText != null)
            {
                banishText.text = _selectedForBanish != null ? "Confirm Banish" : "Banish";
            }
        }

        private void UpdateChargeDisplays()
        {
            if (chargesPanel != null)
            {
                chargesPanel.SetActive(_rerollCharges > 0 || _skipCharges > 0 || _banishCharges > 0);
            }

            if (rerollChargesText != null)
            {
                rerollChargesText.text = $"Rerolls: {_rerollCharges}";
                rerollChargesText.color = _rerollCharges > 0 ? Color.white : Color.gray;
            }

            if (skipChargesText != null)
            {
                skipChargesText.text = $"Skips: {_skipCharges}";
                skipChargesText.color = _skipCharges > 0 ? Color.white : Color.gray;
            }

            if (banishChargesText != null)
            {
                banishChargesText.text = $"Banishes: {_banishCharges}";
                banishChargesText.color = _banishCharges > 0 ? Color.white : Color.gray;
            }
        }

        private void ClearChoices()
        {
            foreach (ChoiceCard card in _activeCards)
            {
                Destroy(card.gameObject);
            }

            _activeCards.Clear();
        }

        private void CreateChoiceCards(List<object> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                GameObject cardObj = Instantiate(choiceCardPrefab, choicesContainer);
                ChoiceCard card = cardObj.GetComponent<ChoiceCard>();

                if (card != null)
                {
                    card.Initialize(choices[i], _player, this);
                    _activeCards.Add(card);
                }
            }
        }

        public void OnChoiceSelected(object choice)
        {
            if (_selectedForBanish != null)
            {
                // If in banish mode, mark for banish instead
                _selectedForBanish = choice;
                HighlightCardForBanish(choice);
                UpdateUI();
            }
            else
            {
                // Normal selection
                _levelUpManager.ApplyChoice(_player, choice);
                CloseLevelUpUI();
            }
        }

        private void HighlightCardForBanish(object choice)
        {
            foreach (ChoiceCard card in _activeCards)
            {
                bool isSelected = card.GetChoice() == choice;
                card.SetBanishHighlight(isSelected);
            }
        }

        private void OnSkipClicked()
        {
            if (_skipCharges <= 0) return;

            _skipCharges--;
            _player.PlayerBaseStats.Skip = Mathf.Max(0, _player.PlayerBaseStats.Skip - 1);
            CloseLevelUpUI();
        }

        private void OnBanishClicked()
        {
            if (_banishCharges <= 0) return;

            if (_selectedForBanish == null)
            {
                // Enter banish selection mode
                _selectedForBanish = new object(); // Placeholder to indicate banish mode
                UpdateUI();
            }
            else
            {
                // Confirm banish
                _levelUpManager.BanishChoice(_selectedForBanish);
                _banishCharges--;
                _player.PlayerBaseStats.Banish = Mathf.Max(0, _player.PlayerBaseStats.Banish - 1);

                // Generate new choices without the banished option
                List<object> newChoices = _levelUpManager.GenerateLevelUpChoices(_player);
                ShowLevelUpChoices(_player, newChoices, _rerollCharges, _skipCharges, _banishCharges);
            }
        }

        private void OnRerollClicked()
        {
            if (_rerollCharges <= 0) return;

            _rerollCharges--;
            _player.PlayerBaseStats.Reroll = Mathf.Max(0, _player.PlayerBaseStats.Reroll - 1);

            List<object> newChoices = _levelUpManager.GenerateLevelUpChoices(_player);
            ShowLevelUpChoices(_player, newChoices, _rerollCharges, _skipCharges, _banishCharges);
        }

        public void ShowTooltip(string text, Vector3 position)
        {
            tooltipText.text = text;
            tooltipPanel.transform.position = position;
            tooltipPanel.SetActive(true);
        }

        public void HideTooltip()
        {
            tooltipPanel.SetActive(false);
        }

        private void CloseLevelUpUI()
        {
            levelUpPanel.SetActive(false);
            Time.timeScale = 1f;
            _selectedForBanish = null;
        }

        // Public getter for banish mode
        public bool IsInBanishMode() => _selectedForBanish != null;
    }
}