using UnityEngine;
using UnityEngine.SceneManagement;

namespace View.UI.Menus
{
    public class PauseManager : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private string sceneName = "MainMenu";
        
        private bool _isPaused;
        
        public bool IsPaused => _isPaused;

        private void Start()
        {
            if (pauseMenuUI)
            {
                pauseMenuUI.SetActive(false);
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
            SceneManager.LoadScene(sceneName);
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