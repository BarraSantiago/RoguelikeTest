using UnityEngine;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

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

        private void ProcessAuthentication(SignInStatus status)
        {
#if UNITY_ANDROID
            if (status == SignInStatus.Success)
            {
                Debug.Log("Signed in successfully");
            }
            else
            {
                Debug.Log($"Sign in failed: {status}");
            }
#endif
        }

        public bool IsAuthenticated()
        {
#if UNITY_ANDROID
            return PlayGamesPlatform.Instance.IsAuthenticated();
#else
                    return false;
#endif
        }
    }
}