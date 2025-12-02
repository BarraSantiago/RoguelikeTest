using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Menus.Achievements
{
    public class ShopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject shopContainer;
        [SerializeField] private Transform shopItemsContainer;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private TextMeshProUGUI totalCoinsText;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        [Header("Tabs")]
        [SerializeField] private Button subclassesTab;
        [SerializeField] private Button statsTab;
        [SerializeField] private Button skinsTab;

        [Header("Popup")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TextMeshProUGUI popupText;
        [SerializeField] private Button popupCloseButton;
        
        [Header("Debug (Remove in Production)")]
        [SerializeField] private Button debugUnlockAllButton;
        [SerializeField] private Button debugClearAllButton;
        
        private ShopUnlockType _currentTab = ShopUnlockType.Subclass;
        private Dictionary<ShopUnlockType, int> _tabItemCounts = new();

        private void Start()
        {
            openButton.onClick.AddListener(TryOpenShop);
            closeButton.onClick.AddListener(() => shopContainer.SetActive(false));
            subclassesTab.onClick.AddListener(() => TryShowTab(ShopUnlockType.Subclass));
            statsTab.onClick.AddListener(() => TryShowTab(ShopUnlockType.StatBonus));
            skinsTab.onClick.AddListener(() => TryShowTab(ShopUnlockType.Skin));

            if (popupCloseButton)
            {
                popupCloseButton.onClick.AddListener(HidePopup);
            }

            if (popupPanel)
            {
                popupPanel.SetActive(false);
            }
            
            if (debugUnlockAllButton)
            {
                debugUnlockAllButton.onClick.AddListener(DebugUnlockAll);
            }

            if (debugClearAllButton)
            {
                debugClearAllButton.onClick.AddListener(DebugClearAll);
            }
        }

        private void OnEnable()
        {
            TryInitializeShop();
        }

        private void OnDisable()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.onShopItemUnlocked.RemoveListener(OnShopItemUnlocked);
            }
        }
        
        private void TryOpenShop()
        {
            if (_tabItemCounts.Values.Sum() == 0)
            {
                ShowShopUnavailablePopup();
                return;
            } 
            shopContainer.SetActive(true);
        }

        private void TryInitializeShop()
        {
            StopAllCoroutines();
            if (AchievementManager.Instance && ShopManager.Instance)
            {
                InitializeShop();
                
                if (AchievementManager.Instance != null)
                {
                    AchievementManager.Instance.onShopItemUnlocked.AddListener(OnShopItemUnlocked);
                }
            }
            else
            {
                StartCoroutine(WaitForManagersAndInitialize());
            }
        }
        
        private void OnShopItemUnlocked()
        {
            if (gameObject.activeInHierarchy)
            {
                // Refresh if shop is open
                TryInitializeShop();
            }
        }
        
        private IEnumerator WaitForManagersAndInitialize()
        {
            yield return new WaitUntil(() => AchievementManager.Instance && ShopManager.Instance);
            InitializeShop();
        }

        private void InitializeShop()
        {
            UpdateCoinsDisplay();
            CountTabItems();

            ShopUnlockType firstAvailableTab = GetFirstAvailableTab();
            ShowTab(firstAvailableTab);
        }

        private void CountTabItems()
        {
            _tabItemCounts.Clear();
            List<ShopUnlock> unlocks = ShopManager.Instance.GetAvailableUnlocks();

            _tabItemCounts[ShopUnlockType.Subclass] = unlocks.Count(u => u.type == ShopUnlockType.Subclass);
            _tabItemCounts[ShopUnlockType.StatBonus] = unlocks.Count(u => u.type == ShopUnlockType.StatBonus);
            _tabItemCounts[ShopUnlockType.Skin] = unlocks.Count(u => u.type == ShopUnlockType.Skin);

            UpdateTabVisuals();
        }

        private void UpdateTabVisuals()
        {
            UpdateTabButton(subclassesTab, ShopUnlockType.Subclass);
            UpdateTabButton(statsTab, ShopUnlockType.StatBonus);
            UpdateTabButton(skinsTab, ShopUnlockType.Skin);
        }

        private void UpdateTabButton(Button tabButton, ShopUnlockType type)
        {
            if (!tabButton) return;

            bool hasItems = _tabItemCounts.ContainsKey(type) && _tabItemCounts[type] > 0;
            
            ColorBlock colors = tabButton.colors;
            colors.normalColor = hasItems ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            tabButton.colors = colors;
        }

        private ShopUnlockType GetFirstAvailableTab()
        {
            if (_tabItemCounts[ShopUnlockType.Subclass] > 0)
                return ShopUnlockType.Subclass;
            
            if (_tabItemCounts[ShopUnlockType.StatBonus] > 0)
                return ShopUnlockType.StatBonus;
            
            if (_tabItemCounts[ShopUnlockType.Skin] > 0)
                return ShopUnlockType.Skin;

            return ShopUnlockType.Subclass;
        }

        private void TryShowTab(ShopUnlockType tab)
        {
            if (!_tabItemCounts.ContainsKey(tab) || _tabItemCounts[tab] == 0)
            {
                ShowTabUnavailablePopup(tab);
                return;
            }

            ShowTab(tab);
        }

        private void ShowTab(ShopUnlockType tab)
        {
            _currentTab = tab;
            PopulateShopItems(tab);
            HighlightActiveTab();
        }

        private void HighlightActiveTab()
        {
            SetTabHighlight(subclassesTab, _currentTab == ShopUnlockType.Subclass);
            SetTabHighlight(statsTab, _currentTab == ShopUnlockType.StatBonus);
            SetTabHighlight(skinsTab, _currentTab == ShopUnlockType.Skin);
        }

        private void SetTabHighlight(Button button, bool highlighted)
        {
            if (!button) return;

            ColorBlock colors = button.colors;
            colors.normalColor = highlighted ? new Color(0.2f, 0.6f, 1f) : Color.white;
            button.colors = colors;
        }

        private void PopulateShopItems(ShopUnlockType filterType)
        {
            foreach (Transform child in shopItemsContainer)
            {
                Destroy(child.gameObject);
            }
        
            if (popupPanel)
            {
                popupPanel.SetActive(false);
            }
        
            List<ShopUnlock> unlocks = ShopManager.Instance.GetAvailableUnlocks();
            List<ShopUnlock> filteredUnlocks = unlocks.Where(u => u.type == filterType).ToList();
        
            foreach (ShopUnlock unlock in filteredUnlocks)
            {
                GameObject item = Instantiate(shopItemPrefab, shopItemsContainer);
                ShopItemUI shopItem = item.GetComponent<ShopItemUI>();
        
                shopItem.Setup(unlock, OnPurchaseItem);
            }
        
            // Resize container after adding items
            StartCoroutine(ResizeContainerAfterFrame());
        }
        
        private IEnumerator ResizeContainerAfterFrame()
        {
            // Wait for layout to update
            yield return new WaitForEndOfFrame();
            
            RectTransform containerRect = shopItemsContainer.GetComponent<RectTransform>();
            
            float totalHeight = 0f;
            
            // Calculate total height of all children
            foreach (RectTransform child in shopItemsContainer)
            {
                totalHeight += child.sizeDelta.y;
            }
            
            // Add spacing if using VerticalLayoutGroup
            VerticalLayoutGroup layoutGroup = shopItemsContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup)
            {
                int childCount = shopItemsContainer.childCount;
                if (childCount > 1)
                {
                    totalHeight += layoutGroup.spacing * (childCount - 1);
                }
                totalHeight += layoutGroup.padding.top + layoutGroup.padding.bottom;
            }
            
            // Set the container size
            containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, totalHeight);
        }
        
        private void ShowShopUnavailablePopup()
        {
            ShowPopup("Shop Unavailable", "Complete achievements to unlock shop items!");
            StartCoroutine(CloseShopAfterPopup());
        }

        private void ShowTabUnavailablePopup(ShopUnlockType tab)
        {
            string tabName = GetTabName(tab);
    
            if (tab == ShopUnlockType.Skin)
            {
                ShowPopup($"{tabName}", "Coming soon!");
            }
            else
            {
                ShowPopup($"{tabName} Unavailable", $"Complete achievements to unlock {tabName.ToLower()}!");
            }
        }

        private string GetTabName(ShopUnlockType type)
        {
            return type switch
            {
                ShopUnlockType.Subclass => "Subclasses",
                ShopUnlockType.StatBonus => "Stat Bonuses",
                ShopUnlockType.Skin => "Skins",
                _ => "Items"
            };
        }

        private void ShowPopup(string title, string message)
        {
            if (!popupPanel || !popupText) return;

            popupText.text = $"<b>{title}</b>\n\n{message}";
            popupPanel.SetActive(true);
        }

        private void HidePopup()
        {
            if (popupPanel)
            {
                popupPanel.SetActive(false);
            }
        }

        private IEnumerator CloseShopAfterPopup()
        {
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !popupPanel.activeSelf);
            gameObject.SetActive(false);
        }

        private void OnPurchaseItem(ShopUnlock unlock)
        {
            if (ShopManager.Instance.PurchaseItem(unlock.id))
            {
                UpdateCoinsDisplay();
                CountTabItems();
                ShowTab(_currentTab);
                Debug.Log($"Purchased: {unlock.targetId} (Level {unlock.currentPurchases}/{unlock.maxPurchases})");
            }
            else
            {
                ShowPopup("Purchase Failed", "Not enough coins or item is already max level!");
            }
        }

        private void UpdateCoinsDisplay()
        {
            if (totalCoinsText)
            {
                totalCoinsText.text = $"Coins: {AchievementManager.Instance.GetTotalCoins()}";
            }
        }
        
        private void DebugUnlockAll()
        {
            if (AchievementManager.Instance)
            {
                AchievementManager.Instance.DebugUnlockAllAchievements();
            }

            if (ShopManager.Instance)
            {
                ShopManager.Instance.DebugUnlockAllShopItems();
            }

            TryInitializeShop();
            ShowPopup("Debug", "All achievements and shop items unlocked!");
        }

        private void DebugClearAll()
        {
            if (AchievementManager.Instance)
            {
                AchievementManager.Instance.DebugClearAllProgress();
            }

            if (ShopManager.Instance)
            {
                ShopManager.Instance.DebugClearAllPurchases();
            }

            TryInitializeShop();
            ShowPopup("Debug", "All progress and purchases cleared!");
        }
    }
}