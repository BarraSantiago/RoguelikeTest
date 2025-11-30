using System.Collections;
using System.Collections.Generic;
using Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Menus.Achievements
{
    public class ShopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform shopItemsContainer;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private TextMeshProUGUI totalCoinsText;
        [SerializeField] private Button closeButton;

        [Header("Tabs")]
        [SerializeField] private Button subclassesTab;
        [SerializeField] private Button statsTab;
        [SerializeField] private Button skinsTab;

        private ShopUnlockType _currentTab = ShopUnlockType.Subclass;

        private void Start()
        {
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            subclassesTab.onClick.AddListener(() => ShowTab(ShopUnlockType.Subclass));
            statsTab.onClick.AddListener(() => ShowTab(ShopUnlockType.StatBonus));
            skinsTab.onClick.AddListener(() => ShowTab(ShopUnlockType.Skin));
        }

        private void OnEnable()
        {
            TryShowTab();
        }

        private void TryShowTab()
        {
            StopAllCoroutines();
            if (AchievementManager.Instance && ShopManager.Instance)
            {
                UpdateCoinsDisplay();
                ShowTab(_currentTab);
            }
            else
            {
                StartCoroutine(WaitForManagersAndShow());
            }
        }

        private IEnumerator WaitForManagersAndShow()
        {
            yield return new WaitUntil(() => AchievementManager.Instance && ShopManager.Instance);
            UpdateCoinsDisplay();
            ShowTab(_currentTab);
        }

        private void ShowTab(ShopUnlockType tab)
        {
            _currentTab = tab;
            PopulateShopItems(tab);
        }

        private void PopulateShopItems(ShopUnlockType filterType)
        {
            foreach (Transform child in shopItemsContainer)
            {
                Destroy(child.gameObject);
            }

            List<ShopUnlock> unlocks = ShopManager.Instance.GetAvailableUnlocks();

            foreach (ShopUnlock unlock in unlocks)
            {
                if (unlock.type != filterType) continue;

                GameObject item = Instantiate(shopItemPrefab, shopItemsContainer);
                ShopItemUI shopItem = item.GetComponent<ShopItemUI>();

                shopItem.Setup(unlock, OnPurchaseItem);
            }
        }

       private void OnPurchaseItem(ShopUnlock unlock)
        {
            if (ShopManager.Instance.PurchaseItem(unlock.id))
            {
                UpdateCoinsDisplay();
                ShowTab(_currentTab); // Refresh display to show new price/level
                Debug.Log($"Purchased: {unlock.targetId} (Level {unlock.currentPurchases}/{unlock.maxPurchases})");
            }
            else
            {
                Debug.Log("Purchase failed!");
            }
        }

        private void UpdateCoinsDisplay()
        {
            totalCoinsText.text = $"Coins: {AchievementManager.Instance.GetTotalCoins()}";
        }
    }
}