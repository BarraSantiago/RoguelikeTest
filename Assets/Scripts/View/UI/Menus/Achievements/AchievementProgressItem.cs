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
        [SerializeField] private Transform milestonesContainer;
        [SerializeField] private GameObject milestonePrefab;
        [SerializeField] private Image completionIcon;

        public void Setup(Achievement achievement, float currentValue)
        {
            achievementNameText.text = achievement.name;
            achievementDescriptionText.text = achievement.description;

            // Clear existing milestones
            foreach (Transform child in milestonesContainer)
            {
                Destroy(child.gameObject);
            }

            // Create milestone progress bars
            foreach (AchievementMilestone milestone in achievement.milestones)
            {
                GameObject milestoneObj = Instantiate(milestonePrefab, milestonesContainer);
                AchievementMilestoneItem milestoneItem = milestoneObj.GetComponent<AchievementMilestoneItem>();

                float progress = Mathf.Clamp01(currentValue / milestone.targetValue);
                milestoneItem.Setup(milestone, progress, currentValue);
            }

            // Show completion icon if fully completed
            completionIcon.gameObject.SetActive(achievement.isUnlocked);
        }
    }
}