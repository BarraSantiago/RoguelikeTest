using System.Collections.Generic;
using PlayerDirectory;
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
                
                [Header("Settings")]
                [SerializeField] private int rerollCost = 50;
                
                private Player _player;
                private LevelUpManager _levelUpManager;
                private List<ChoiceCard> _activeCards = new List<ChoiceCard>();
                private List<object> _currentChoices;
        
                private void Awake()
                {
                    skipButton.onClick.AddListener(OnSkipClicked);
                    banishButton.onClick.AddListener(OnBanishClicked);
                    rerollButton.onClick.AddListener(OnRerollClicked);
                    
                    tooltipPanel.SetActive(false);
                    levelUpPanel.SetActive(false);
                }
        
                public void ShowLevelUpChoices(Player player, List<object> choices)
                {
                    _player = player;
                    _currentChoices = choices;
                    _levelUpManager = FindFirstObjectByType<LevelUpManager>();
                    
                    ClearChoices();
                    CreateChoiceCards(choices);
                    
                    levelUpPanel.SetActive(true);
                    Time.timeScale = 0f;
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
                    _levelUpManager.ApplyChoice(_player, choice);
                    CloseLevelUpUI();
                }
        
                private void OnSkipClicked()
                {
                    CloseLevelUpUI();
                }
        
                private void OnBanishClicked()
                {
                    // TODO: Implement banish logic (remove option from future selections)
                    CloseLevelUpUI();
                }
        
                private void OnRerollClicked()
                {
                    if (_player.levelSystem.CurrentExperience >= rerollCost)
                    {
                        _player.levelSystem.AddExperience(-rerollCost);
                        List<object> newChoices = _levelUpManager.GenerateLevelUpChoices(_player);
                        ShowLevelUpChoices(_player, newChoices);
                    }
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
                }
            }
        }