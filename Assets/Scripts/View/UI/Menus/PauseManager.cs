using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace View.UI.Menus
{
    public class PauseManager : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button changeSceneButton;
        [SerializeField] private string sceneName = "MainMenu";
        [SerializeField] private bool useLoadingScreen = true;

        private bool _isPaused;

        public bool IsPaused => _isPaused;

        private void Start()
        {
            if (pauseMenuUI)
            {
                pauseMenuUI.SetActive(false);
                if(pauseButton) pauseButton.onClick.AddListener(TogglePause);
            }
            
            if(changeSceneButton)
            {
                changeSceneButton.onClick.AddListener(ChangeScene);
            }
        }

        public void TogglePause()
        {
            if (_isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;
            if (pauseMenuUI)
            {
                pauseMenuUI.SetActive(true);
            }
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            if (pauseMenuUI)
            {
                pauseMenuUI.SetActive(false);
            }
        }

        public void ChangeScene()
        {
            Time.timeScale = 1f;

            if (useLoadingScreen)
            {
                LoadingSceneManager.LoadScene(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        public void ChangeScene(string _sceneName, bool _useLoadingScreen = true)
        {
            Time.timeScale = 1f;

            if (_useLoadingScreen)
            {
                LoadingSceneManager.LoadScene(_sceneName);
            }
            else
            {
                SceneManager.LoadScene(_sceneName);
            }
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}