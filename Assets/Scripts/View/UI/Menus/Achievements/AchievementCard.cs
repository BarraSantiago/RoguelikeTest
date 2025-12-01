using Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Menus.Achievements
{
    public class AchievementCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private AchievementMilestoneItem currentMilestoneDisplay;
        [SerializeField] private Image completionBadge;
        [SerializeField] private TextMeshProUGUI progressInfoText;
        [SerializeField] private TextMeshProUGUI totalRewardText;

        public void Setup(Achievement achievement, float currentValue)
        {
            titleText.text = achievement.name;
            descriptionText.text = achievement.description;

            // Find current active milestone (first incomplete one)
            AchievementMilestone currentMilestone = null;
            int completedCount = 0;

            foreach (AchievementMilestone milestone in achievement.milestones)
            {
                if (milestone.isCompleted)
                {
                    completedCount++;
                }
                else if (currentMilestone == null)
                {
                    currentMilestone = milestone;
                }
            }

            // Show progress info
            progressInfoText.text = $"{completedCount}/{achievement.milestones.Count}";

            // Display current milestone or completion status
            if (achievement.isUnlocked)
            {
                currentMilestoneDisplay.gameObject.SetActive(false);
                completionBadge.gameObject.SetActive(true);
                totalRewardText.text = $"COMPLETED! Total earned: {achievement.coinReward} coins";
            }
            else if (currentMilestone != null)
            {
                currentMilestoneDisplay.gameObject.SetActive(true);
                completionBadge.gameObject.SetActive(false);

                float progress = Mathf.Clamp01(currentValue / currentMilestone.targetValue);
                currentMilestoneDisplay.Setup(currentMilestone, progress, currentValue);

                int totalCoins = 0;
                foreach (AchievementMilestone m in achievement.milestones)
                    totalCoins += m.coinReward;
                totalRewardText.text = $"Total rewards: {totalCoins} coins";
            }
        }
    }
}