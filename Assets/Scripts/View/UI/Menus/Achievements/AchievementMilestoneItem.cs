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
        [SerializeField] private Color completedColor = Color.green;
        [SerializeField] private Color incompleteColor = Color.gray;

        public void Setup(AchievementMilestone milestone, float progress, float currentValue)
        {
            progressBar.value = progress;
            targetText.text = $"{currentValue:F0} / {milestone.targetValue}";

            // Show reward
            string reward = $"+{milestone.coinReward} coins";
            if (milestone.shopUnlock != null)
            {
                reward += $" | Unlocks: {milestone.shopUnlock.targetId}";
            }
            rewardText.text = reward;

            // Visual feedback for completion
            bool isCompleted = milestone.isCompleted;
            completionCheckmark.gameObject.SetActive(isCompleted);
            progressBar.fillRect.GetComponent<Image>().color = isCompleted ? completedColor : incompleteColor;

            // Dim text if completed
            Color textColor = isCompleted ? new Color(1, 1, 1, 0.6f) : Color.white;
            targetText.color = textColor;
            rewardText.color = textColor;
        }
    }
}