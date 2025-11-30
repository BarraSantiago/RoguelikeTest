using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Menus.Achievements
{
    public class SubclassProgressItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI subclassNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image progressBar;
        [SerializeField] private GameObject unlockedIcon;
        [SerializeField] private Color lockedColor = Color.red;
        [SerializeField] private Color unlockedColor = Color.green;

        public void Setup(string subclassName, string description, float progress, bool isUnlocked)
        {
            subclassNameText.text = subclassName;
            descriptionText.text = description;
            progressText.text = isUnlocked ? "UNLOCKED" : $"{Mathf.RoundToInt(progress * 100)}%";
            progressBar.fillAmount = progress;
            progressBar.color = isUnlocked ? unlockedColor : Color.Lerp(lockedColor, unlockedColor, progress);
            
            if (unlockedIcon != null)
                unlockedIcon.SetActive(isUnlocked);
        }
    }
}