using System.Collections.Generic;
using Classes.Evolution;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Leveling
{
    public class SubclassSelectionUI : MonoBehaviour
    {
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private SubclassEvolutionManager _evolutionManager;

        public void ShowSubclasses(List<string> availableSubclasses, Dictionary<string, UnlockCondition> conditions, SubclassEvolutionManager manager)
        {
            _evolutionManager = manager;
            titleText.text = "Choose Your Path";
            descriptionText.text = "Select your subclass evolution";

            ClearButtons();

            foreach (string subclass in availableSubclasses)
            {
                CreateButton(subclass, conditions[subclass].description);
            }
        }

        private void ClearButtons()
        {
            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void CreateButton(string subclass, string description)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            buttonText.text = $"{subclass}\n<size=14>{description}</size>";
            button.onClick.AddListener(() => OnSubclassSelected(subclass));
        }

        private void OnSubclassSelected(string subclass)
        {
            _evolutionManager.EvolveToSubclass(subclass);
        }
    }
}