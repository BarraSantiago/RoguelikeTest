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
        [SerializeField] private Transform milestonesContainer;
        [SerializeField] private GameObject milestonePrefab;
        [SerializeField] private Image completionBadge;
        [SerializeField] private TextMeshProUGUI totalRewardText;

        public void Setup(Achievement achievement, float currentValue)
        {
            titleText.text = achievement.name;
            descriptionText.text = achievement.description;
            totalRewardText.text = $"Total Reward: {achievement.coinReward} coins";

            foreach (Transform child in milestonesContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (AchievementMilestone milestone in achievement.milestones)
            {
                GameObject milestoneObj = Instantiate(milestonePrefab, milestonesContainer);
                AchievementMilestoneItem item = milestoneObj.GetComponent<AchievementMilestoneItem>();

                float progress = Mathf.Clamp01(currentValue / milestone.targetValue);
                item.Setup(milestone, progress, currentValue);
            }

            completionBadge.gameObject.SetActive(achievement.isUnlocked);
        }
    }
}