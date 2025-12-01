using GooglePlayGames;
using UnityEngine;
using GooglePlayGames.BasicApi;

namespace GooglePlay
{
    public class GooglePlayManager : MonoBehaviour
    {
        private static GooglePlayManager _instance;
        public static GooglePlayManager Instance => _instance;

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeGooglePlay();
            SignIn();
        }

        private void InitializeGooglePlay()
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Activate();
            Debug.Log("Google Play Games initialized");
#endif
        }

        public void SignIn()
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
#endif
        }

#if UNITY_ANDROID
        private void ProcessAuthentication(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                Achievement.UnlockAchievement(GPGSIds.achievement_start_the_game);
                Debug.Log("Signed in successfully");
            }
            else
            {
                Debug.Log($"Sign in failed: {status}");
            }
        }
#endif

        public bool IsAuthenticated()
        {
#if UNITY_ANDROID
            return PlayGamesPlatform.Instance.IsAuthenticated();
#else
                    return false;
#endif
        }
        
        public void SubmitKillsScore(int kills)
        {
            if (IsAuthenticated())
            {
                Achievement.SubmitScore(GPGSIds.leaderboard_kills_this_session, kills);
            }
        }
        
        public void ShowKillsLeaderboard()
        {
            if (IsAuthenticated())
            {
                Achievement.ShowLeaderboardUI(GPGSIds.leaderboard_kills_this_session);
            }
        }
    }
}