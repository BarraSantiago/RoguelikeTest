using UnityEngine;
#if UNITY_ANDROID
using GooglePlayGames;
#endif

namespace GooglePlay
{
    public class Achievement
    {
        public static void UnlockAchievement(string achievementId)
        {
#if UNITY_ANDROID
            if (PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.UnlockAchievement(achievementId, success =>
                {
                    Debug.Log(success
                        ? $"Achievement {achievementId} unlocked"
                        : $"Failed to unlock achievement {achievementId}");
                });
            }
#endif
        }

        public static void IncrementAchievement(string achievementId, int steps)
        {
#if UNITY_ANDROID
            if (PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.IncrementAchievement(achievementId, steps, success =>
                {
                    Debug.Log(success
                        ? $"Achievement {achievementId} incremented by {steps}"
                        : $"Failed to increment achievement {achievementId}");
                });
            }
#endif
        }

        public static void RevealAchievement(string achievementId)
        {
#if UNITY_ANDROID
            if (PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.RevealAchievement(achievementId, success =>
                {
                    Debug.Log(success
                        ? $"Achievement {achievementId} revealed"
                        : $"Failed to reveal achievement {achievementId}");
                });
            }
#endif
        }

        public static void ShowAchievementsUI()
        {
#if UNITY_ANDROID
            if (PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.ShowAchievementsUI();
            }
#endif
        }
        
        public static void SubmitScore(string leaderboardId, long score)
        {
        #if UNITY_ANDROID
            if (PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.ReportScore(score, leaderboardId, success =>
                {
                    Debug.Log(success
                        ? $"Score {score} submitted to leaderboard {leaderboardId}"
                        : $"Failed to submit score to leaderboard {leaderboardId}");
                });
            }
        #endif
        }
        
        public static void ShowLeaderboardUI(string leaderboardId = null)
        {
        #if UNITY_ANDROID
            if (PlayGamesPlatform.Instance.IsAuthenticated())
            {
                if (string.IsNullOrEmpty(leaderboardId))
                {
                    PlayGamesPlatform.Instance.ShowLeaderboardUI();
                }
                else
                {
                    PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardId);
                }
            }
        #endif
        }
    }
}