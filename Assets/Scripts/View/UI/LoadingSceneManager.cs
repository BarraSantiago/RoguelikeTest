using System.Collections;
using System.Collections.Generic;
using ClassesDirectory;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace View.UI
{
    public class LoadingSceneManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI loadingTipText;
        [SerializeField] private GameObject classSelectionPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Button continueButton;

        [Header("Class Selection")]
        [SerializeField] private ClassSelectionButton[] classButtons;
        [SerializeField] private Transform subclassContainer;
        [SerializeField] private GameObject subclassButtonPrefab;

        [Header("Loading Settings")] 
        [SerializeField] private float minimumLoadTime = 1.5f;
        [SerializeField] private float artificialDelay = 0.5f;

        [Header("Loading Tips")]
        [SerializeField]
        private string[] loadingTips = new[]
        {
            "Tip: Dodge enemy attacks to survive longer!",
            "Tip: Collect XP gems to level up and get stronger!",
            "Tip: Elite enemies drop more XP but are tougher!",
            "Tip: Try different classes for unique playstyles!",
            "Tip: Boss enemies appear in later waves - be prepared!",
            "Tip: Keep moving to avoid being surrounded!"
        };

        private static string _sceneToLoad;
        private static Classes _selectedClass = Classes.Warrior;
        private static string _selectedSubclass = "None";
        private List<Button> _subclassButtons = new();

        public static Classes SelectedClass => _selectedClass;
        public static string SelectedSubclass => _selectedSubclass;

        private void Start()
        {
            if (string.IsNullOrEmpty(_sceneToLoad))
            {
                Debug.LogError("No scene set to load!");
                return;
            }

            SetupClassSelection();

            if (loadingTipText && loadingTips.Length > 0)
            {
                loadingTipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            }

            if (continueButton)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
                continueButton.gameObject.SetActive(false);
            }

            if (classSelectionPanel)
            {
                classSelectionPanel.SetActive(true);
            }

            if (loadingPanel)
            {
                loadingPanel.SetActive(true);
            }

            StartCoroutine(LoadSceneAsync());
        }

        public static void LoadScene(string sceneName)
        {
            _sceneToLoad = sceneName;
            _selectedClass = Classes.Warrior;
            _selectedSubclass = "None";
            SceneManager.LoadScene("LoadingScene");
        }

        private void SetupClassSelection()
        {
            foreach (ClassSelectionButton classButton in classButtons)
            {
                classButton.button.onClick.AddListener(() => OnClassSelected(classButton));
            }

            SelectClass(Classes.Warrior);
        }

        private void OnClassSelected(ClassSelectionButton selectedButton)
        {
            foreach (ClassSelectionButton button in classButtons)
            {
                button.SetSelected(false);
            }

            selectedButton.SetSelected(true);
            _selectedClass = selectedButton.classType;
            _selectedSubclass = "None";

            UpdateSubclassOptions(selectedButton.classType);
        }

        private void UpdateSubclassOptions(Classes classType)
        {
            foreach (Button button in _subclassButtons)
            {
                if (button) Destroy(button.gameObject);
            }

            _subclassButtons.Clear();

            if (!subclassContainer || !subclassButtonPrefab) return;

            List<string> unlockedSubclasses = GetUnlockedSubclasses(classType);

            Button noneButton = CreateSubclassButton("Base Class", true);
            noneButton.onClick.AddListener(() => OnSubclassSelected("None", noneButton));
            noneButton.interactable = true;
            SetSubclassButtonSelected(noneButton, true);

            foreach (string subclass in unlockedSubclasses)
            {
                Button button = CreateSubclassButton(subclass, true);
                button.onClick.AddListener(() => OnSubclassSelected(subclass, button));
            }

            List<string> lockedSubclasses = GetLockedSubclasses(classType);
            foreach (string subclass in lockedSubclasses)
            {
                Button button = CreateSubclassButton($"{subclass} (Locked)", false);
                button.interactable = false;
            }
        }

        private Button CreateSubclassButton(string subclassName, bool unlocked)
        {
            GameObject buttonObj = Instantiate(subclassButtonPrefab, subclassContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (text)
            {
                text.text = subclassName;
                text.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            _subclassButtons.Add(button);
            return button;
        }

        private void OnSubclassSelected(string subclass, Button selectedButton)
        {
            _selectedSubclass = subclass;

            foreach (Button button in _subclassButtons)
            {
                SetSubclassButtonSelected(button, false);
            }

            SetSubclassButtonSelected(selectedButton, true);
        }

        private void SetSubclassButtonSelected(Button button, bool selected)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = selected ? new Color(0.2f, 0.6f, 1f) : Color.white;
            button.colors = colors;
        }

        private void SelectClass(Classes classType)
        {
            foreach (ClassSelectionButton button in classButtons)
            {
                if (button.classType == classType)
                {
                    OnClassSelected(button);
                    break;
                }
            }
        }

        private List<string> GetUnlockedSubclasses(Classes classType)
        {
            List<string> unlocked = new();
        
            switch (classType)
            {
                case Classes.Warrior:
                    if (PlayerPrefs.GetInt("SubclassUnlocked_Berserker", 0) == 1)
                        unlocked.Add("Berserker");
                    if (PlayerPrefs.GetInt("SubclassUnlocked_Paladin", 0) == 1)
                        unlocked.Add("Paladin");
                    if (PlayerPrefs.GetInt("SubclassUnlocked_Reaver", 0) == 1)
                        unlocked.Add("Reaver");
                    break;
        
                case Classes.Ranger:
                    if (PlayerPrefs.GetInt("SubclassUnlocked_Sniper", 0) == 1)
                        unlocked.Add("Sniper");
                    if (PlayerPrefs.GetInt("SubclassUnlocked_Hunter", 0) == 1)
                        unlocked.Add("Hunter");
                    if (PlayerPrefs.GetInt("SubclassUnlocked_Trapper", 0) == 1)
                        unlocked.Add("Trapper");
                    break;
            }
        
            return unlocked;
        }

        private List<string> GetLockedSubclasses(Classes classType)
        {
            List<string> locked = new();
            List<string> unlocked = GetUnlockedSubclasses(classType);

            switch (classType)
            {
                case Classes.Warrior:
                    if (!unlocked.Contains("Berserker"))
                        locked.Add("Berserker");
                    if (!unlocked.Contains("Paladin"))
                        locked.Add("Paladin");
                    if (!unlocked.Contains("Reaver"))
                        locked.Add("Reaver");
                    break;

                case Classes.Ranger:
                    if (!unlocked.Contains("Sniper"))
                        locked.Add("Sniper");
                    if (!unlocked.Contains("Hunter"))
                        locked.Add("Hunter");
                    if (!unlocked.Contains("Trapper"))
                        locked.Add("Trapper");
                    break;
            }

            return locked;
        }

        private void OnContinueClicked()
        {
            if (_loadedOperation != null)
            {
                _loadedOperation.allowSceneActivation = true;
            }
        }

        private AsyncOperation _loadedOperation;

        private IEnumerator LoadSceneAsync()
        {
            float startTime = Time.realtimeSinceStartup;

            yield return new WaitForSeconds(artificialDelay);

            AsyncOperation operation = SceneManager.LoadSceneAsync(_sceneToLoad);
            operation.allowSceneActivation = false;
            _loadedOperation = operation;

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                float elapsedTime = Time.realtimeSinceStartup - startTime;

                if (progressBar)
                {
                    progressBar.value = progress;
                }

                if (progressText)
                {
                    progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
                }

                if (operation.progress >= 0.9f && elapsedTime >= minimumLoadTime)
                {
                    if (progressBar)
                    {
                        progressBar.value = 1f;
                        progressBar.gameObject.SetActive(false);
                    }

                    if (progressText)
                    {
                        progressText.text = "100%";
                        yield return new WaitForSeconds(0.3f);
                        progressText.gameObject.SetActive(false);
                    }

                    if (continueButton)
                    {
                        continueButton.gameObject.SetActive(true);
                    }

                    yield break;
                }

                yield return null;
            }
        }
    }


    [System.Serializable]
    public class ClassSelectionButton
    {
        public Button button;
        public Classes classType;
        public GameObject selectedIndicator;

        public void SetSelected(bool selected)
        {
            if (selectedIndicator)
            {
                selectedIndicator.SetActive(selected);
            }

            ColorBlock colors = button.colors;
            colors.normalColor = selected ? new Color(0.2f, 0.6f, 1f) : Color.white;
            button.colors = colors;
        }
    }
}