using Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Menus.Achievements
{
    public class AchievementProgressItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI achievementNameText;
        [SerializeField] private TextMeshProUGUI achievementDescriptionText;
        [SerializeField] private AchievementMilestoneItem currentMilestoneDisplay;
        [SerializeField] private Image completionIcon;
        [SerializeField] private TextMeshProUGUI milestoneCounterText;

        public void Setup(Achievement achievement, float currentValue)
        {
            achievementNameText.text = achievement.name;
            achievementDescriptionText.text = achievement.description;

            // Find current active milestone
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

            // Update milestone counter
            milestoneCounterText.text = $"{completedCount}/{achievement.milestones.Count}";

            // Display current milestone or show completion
            if (achievement.isUnlocked)
            {
                currentMilestoneDisplay.gameObject.SetActive(false);
                completionIcon.gameObject.SetActive(true);
                achievementDescriptionText.text = "COMPLETED!";
            }
            else if (currentMilestone != null)
            {
                currentMilestoneDisplay.gameObject.SetActive(true);
                completionIcon.gameObject.SetActive(false);

                float progress = Mathf.Clamp01(currentValue / currentMilestone.targetValue);
                currentMilestoneDisplay.Setup(currentMilestone, progress, currentValue);
            }
        }
    }
}