using System;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Menus
{
    public class ChangeScene : MonoBehaviour
    {
        [SerializeField] private Button changeSceneButton;
        [SerializeField] private string sceneName = "MainMenu";
        [SerializeField] private bool useLoadingScreen = true;

        private void Awake()
        {
            if (changeSceneButton)
            {
                changeSceneButton.onClick.AddListener(ChangeSceneFunc);
            }
        }
        
        public void ChangeSceneFunc()
        {
            if (useLoadingScreen)
            {
                //PauseManager.instance.LoadSceneWithLoadingScreen(sceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }
    }
}