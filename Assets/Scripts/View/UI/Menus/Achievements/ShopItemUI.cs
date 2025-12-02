using System;
using Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Menus.Achievements
{
    public class ShopItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI purchaseCountText;
        [SerializeField] private Transform purchaseIndicatorsContainer;
        [SerializeField] private GameObject purchaseIndicatorPrefab;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private GameObject purchasedOverlay;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Color purchasedIndicatorColor = Color.green;
        [SerializeField] private Color unpurchasedIndicatorColor = Color.gray;

        private ShopUnlock _unlock;
        private Action<ShopUnlock> _onPurchase;

        public void Setup(ShopUnlock unlock, Action<ShopUnlock> onPurchase)
        {
            _unlock = unlock;
            _onPurchase = onPurchase;

            itemNameText.text = GetItemName(unlock);
            itemDescriptionText.text = GetItemDescription(unlock);

            // Show current cost and purchase count
            if (unlock.maxPurchases > 1)
            {
                costText.text = unlock.CanPurchaseMore()
                    ? $"{unlock.currentCost} coins"
                    : "MAX LEVEL";
                purchaseCountText.text = $"Level {unlock.currentPurchases}/{unlock.maxPurchases}";
                purchaseCountText.gameObject.SetActive(true);

                // Show purchase indicators
                SetupPurchaseIndicators(unlock);
            }
            else
            {
                costText.text = $"{unlock.currentCost} coins";
                purchaseCountText.gameObject.SetActive(false);
                if (purchaseIndicatorsContainer)
                    purchaseIndicatorsContainer.gameObject.SetActive(false);
            }

            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(OnPurchaseClicked);

            bool fullyPurchased = !unlock.CanPurchaseMore();
            purchasedOverlay.SetActive(fullyPurchased);

            bool canAfford = AchievementManager.Instance.GetTotalCoins() >= unlock.currentCost;
            purchaseButton.interactable = !fullyPurchased && canAfford;

            // Update button text
            TextMeshProUGUI buttonText = purchaseButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText)
            {
                if (fullyPurchased)
                    buttonText.text = "MAXED";
                else if (unlock.currentPurchases > 0)
                    buttonText.text = "UPGRADE";
                else
                    buttonText.text = "BUY";
            }
        }

        private void SetupPurchaseIndicators(ShopUnlock unlock)
        {
            if (!purchaseIndicatorsContainer || !purchaseIndicatorPrefab)
                return;

            // Clear existing indicators
            foreach (Transform child in purchaseIndicatorsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create indicator for each possible purchase
            for (int i = 0; i < unlock.maxPurchases; i++)
            {
                GameObject indicator = Instantiate(purchaseIndicatorPrefab, purchaseIndicatorsContainer);
                Image indicatorImage = indicator.GetComponent<Image>();

                if (indicatorImage)
                {
                    indicatorImage.color = i < unlock.currentPurchases
                        ? purchasedIndicatorColor
                        : unpurchasedIndicatorColor;
                }
            }
        }

        private void OnPurchaseClicked()
        {
            _onPurchase?.Invoke(_unlock);
        }

        private string GetItemName(ShopUnlock unlock)
        {
            StatBonus bonus = ShopManager.Instance.GetStatBonus(unlock.targetId);

            return unlock.type switch
            {
                ShopUnlockType.Subclass => unlock.targetId,
                ShopUnlockType.StatBonus => bonus?.name ?? unlock.targetId,
                ShopUnlockType.Skin => unlock.targetId,
                _ => "Unknown"
            };
        }

        private string GetItemDescription(ShopUnlock unlock)
        {
            StatBonus bonus = ShopManager.Instance.GetStatBonus(unlock.targetId);

            return unlock.type switch
            {
                ShopUnlockType.Subclass => $"Unlock the {unlock.targetId} subclass",
                ShopUnlockType.StatBonus => bonus?.description ?? "",
                ShopUnlockType.Skin => $"Cosmetic skin: {unlock.targetId}",
                _ => ""
            };
        }
    }
}