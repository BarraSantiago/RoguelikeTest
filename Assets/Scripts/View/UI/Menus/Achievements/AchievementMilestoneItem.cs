using Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Menus.Achievements
{
    public class AchievementMilestoneItem : MonoBehaviour
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Image completionCheckmark;
        [SerializeField] private TextMeshProUGUI percentageText;
        [SerializeField] private TextMeshProUGUI sessionProgressText;
        [SerializeField] private Color completedColor = Color.green;
        [SerializeField] private Color incompleteColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color progressBarBackgroundColor = new Color(0.2f, 0.2f, 0.2f);

        public void Setup(AchievementMilestone milestone, float progress, float currentValue, float sessionProgress = 0)
        {
            progressBar.value = progress;

            // Format numbers based on their size
            string currentStr = FormatNumber(currentValue);
            string targetStr = FormatNumber(milestone.targetValue);
            targetText.text = $"{currentStr} / {targetStr}";

            // Show percentage
            if (percentageText)
            {
                percentageText.text = $"{(progress * 100):F0}%";
            }

            // Show reward with better formatting
            string reward = $"Reward: {milestone.coinReward} <sprite=0>"; // Use coin sprite if you have one
            if (milestone.shopUnlock != null)
            {
                string unlockName = GetFriendlyUnlockName(milestone.shopUnlock);
                reward += $"\nUnlocks: {unlockName}";
            }

            rewardText.text = reward;

            // Visual feedback for completion
            bool isCompleted = milestone.isCompleted;
            completionCheckmark.gameObject.SetActive(isCompleted);

            Image fillImage = progressBar.fillRect.GetComponent<Image>();
            if (fillImage)
            {
                fillImage.color = isCompleted ? completedColor : incompleteColor;
            }

            Image backgroundImage = progressBar.GetComponentsInChildren<Image>()[0];
            if (backgroundImage)
            {
                backgroundImage.color = progressBarBackgroundColor;
            }

            // Adjust text opacity if completed
            float alpha = isCompleted ? 0.6f : 1f;
            targetText.color = new Color(1, 1, 1, alpha);
            rewardText.color = new Color(1, 1, 1, alpha);
            if (percentageText)
                percentageText.color = new Color(1, 1, 1, alpha);
            
            if (sessionProgress > 0 && sessionProgressText)
            {
                sessionProgressText.gameObject.SetActive(true);
                sessionProgressText.text = $"+{sessionProgress:F0} this session";
            }
            else if (sessionProgressText)
            {
                sessionProgressText.gameObject.SetActive(false);
            }
        }

        private string FormatNumber(float value)
        {
            if (value >= 1000000)
                return $"{value / 1000000f:F1}M";
            if (value >= 1000)
                return $"{value / 1000f:F1}K";
            return $"{value:F0}";
        }

        private string GetFriendlyUnlockName(ShopUnlock unlock)
        {
            return unlock.type switch
            {
                ShopUnlockType.Subclass => unlock.targetId,
                ShopUnlockType.StatBonus => ShopManager.Instance.GetStatBonus(unlock.targetId)?.name ?? unlock.targetId,
                ShopUnlockType.Skin => unlock.targetId,
                _ => unlock.targetId
            };
        }
    }
}