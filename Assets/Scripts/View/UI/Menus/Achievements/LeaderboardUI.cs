using UnityEngine;
using UnityEngine.UI;
using GooglePlay;
using TMPro;

namespace UI
{
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private Button showLeaderboardButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private GameObject notAuthenticatedMessage;

        private void Start()
        {
            showLeaderboardButton?.onClick.AddListener(OnShowLeaderboardClicked);
            achievementsButton?.onClick.AddListener(OnShowAchievementsClicked);
            //UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            bool isAuthenticated = GooglePlayManager.Instance.IsAuthenticated();
            showLeaderboardButton.interactable = isAuthenticated;

            if (notAuthenticatedMessage != null)
            {
                notAuthenticatedMessage.SetActive(!isAuthenticated);
            }

            if (buttonText != null)
            {
                buttonText.text = isAuthenticated ? "View Leaderboard" : "Sign In Required";
            }
        }

        private void OnShowLeaderboardClicked()
        {
            if (notAuthenticatedMessage != null)
            {
                OnSignInClicked();
                bool isAuthenticated = GooglePlayManager.Instance.IsAuthenticated();
                showLeaderboardButton.interactable = isAuthenticated;
            }
            else
            {
                GooglePlayManager.Instance.ShowKillsLeaderboard();
            }
        }
        
        private void OnShowAchievementsClicked()
        {
            if (notAuthenticatedMessage != null)
            {
                OnSignInClicked();
                bool isAuthenticated = GooglePlayManager.Instance.IsAuthenticated();
                showLeaderboardButton.interactable = isAuthenticated;
            }
            else
            {
                GooglePlayManager.Instance.ShowAchievementsUI();
            }
        }

        public void OnSignInClicked()
        {
            GooglePlayManager.Instance.SignInManual();
            Invoke(nameof(UpdateButtonState), 1f);
        }
    }
}