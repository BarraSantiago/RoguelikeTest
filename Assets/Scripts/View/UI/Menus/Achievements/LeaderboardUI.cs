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
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private GameObject notAuthenticatedMessage;

        private void Start()
        {
            showLeaderboardButton.onClick.AddListener(OnShowLeaderboardClicked);
            UpdateButtonState();
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
            GooglePlayManager.Instance.ShowKillsLeaderboard();
        }

        public void OnSignInClicked()
        {
            GooglePlayManager.Instance.SignIn();
            Invoke(nameof(UpdateButtonState), 1f);
        }
    }
}