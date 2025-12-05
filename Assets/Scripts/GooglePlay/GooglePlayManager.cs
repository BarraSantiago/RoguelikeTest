using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

namespace GooglePlay
{
    public class GooglePlayManager : MonoBehaviour
    {
        private static GooglePlayManager _instance;
        public static GooglePlayManager Instance => _instance;

        private void Awake()
        {
            if (_instance && _instance!= this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeGooglePlay();
            
            // Intentamos el login silencioso automáticamente al arrancar
            SignInSilent();
        }

        private void InitializeGooglePlay()
        {
#if UNITY_ANDROID
            // Recomendado para depurar errores de configuración (SHA-1, Package Name) 
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
            Debug.Log("Google Play Games initialized");
#endif
        }

        /// <summary>
        /// Intento automático y silencioso. NO muestra UI si falla.
        /// Debe llamarse en Start/Awake.
        /// </summary>
        public void SignInSilent()
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
#endif
        }

        /// <summary>
        /// Intento manual e interactivo. MUESTRA la UI de Google para elegir cuenta.
        /// Debe enlazarse a un botón de "Sign In" en tu menú si SignInSilent falla.
        /// Este es el método que faltaba para completar el flujo en v2.1.0.
        /// </summary>
        public void SignInManual()
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
#endif
        }

#if UNITY_ANDROID
        private void ProcessAuthentication(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                // Asegurar que el código se ejecute en el hilo principal si tocas UI
                // En versiones recientes el plugin suele hacer esto, pero es mejor prevenir.
                Debug.Log($"Signed in successfully. User: {PlayGamesPlatform.Instance.GetUserDisplayName()}");

                // Opcional: Si necesitas tokens para un servidor backend (Firebase/PlayFab),
                // debes solicitar el acceso al servidor aquí.[3]
                // RequestServerAccess(); 
                
                Achievement.UnlockAchievement(GPGSIds.achievement_start_the_game);
            }
            else
            {
                // Si el status es Canceled o Failed, aquí es donde deberías activar
                // visualmente tu botón de "Conectar con Google" en la UI del juego,
                // para que el usuario pueda llamar a SignInManual().
                Debug.Log($"Sign in failed or canceled. Status: {status}");
            }
        }
        
        // Ejemplo de cómo obtener el Auth Code si tuvieras un backend (Opcional)
        /*
        private void RequestServerAccess()
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, code => 
            {
                Debug.Log("Server Auth Code: " + code);
            });
        }
        */
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