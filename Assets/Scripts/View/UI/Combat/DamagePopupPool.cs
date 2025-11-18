using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Combat
{
    public class DamagePopupPool : MonoBehaviour
    {
        [Header("Prefab")] 
        [SerializeField] private GameObject damagePopupPrefab;
        
        [Header("Pool Settings")] 
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private Transform poolContainer;

        [Header("Configuration")] 
        [SerializeField] private Canvas _worldCanvas;
        [SerializeField] private bool enablePopups = true;
        [SerializeField] private float damageThreshold; // Minimum damage to show
        [SerializeField] private int maxActivePopups = 50;

        private Queue<DamagePopup> _availablePopups = new();
        private List<DamagePopup> _activePopups = new();
        

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            if (!poolContainer)
            {
                poolContainer = new GameObject("Popup Pool").transform;
                poolContainer.SetParent(_worldCanvas.transform);
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewPopup();
            }
        }

        private DamagePopup CreateNewPopup()
        {
            GameObject popupObj = Instantiate(damagePopupPrefab, poolContainer);
            DamagePopup popup = popupObj.GetComponent<DamagePopup>();
            popupObj.SetActive(false);
            _availablePopups.Enqueue(popup);
            return popup;
        }

        public void ShowDamage(Vector3 position, float damage, bool isCritical = false)
        {
            if (!enablePopups || damage < damageThreshold) return;
            if (_activePopups.Count >= maxActivePopups) return;

            DamagePopup popup = GetPopup();
            popup.gameObject.SetActive(true);
            popup.Initialize(this, position, damage, isCritical);
            _activePopups.Add(popup);
        }

        private DamagePopup GetPopup()
        {
            if (_availablePopups.Count == 0)
            {
                return CreateNewPopup();
            }

            return _availablePopups.Dequeue();
        }

        public void ReturnToPool(DamagePopup popup)
        {
            _activePopups.Remove(popup);
            popup.gameObject.SetActive(false);
            _availablePopups.Enqueue(popup);
        }

        // Public configuration methods
        public void SetEnabled(bool enabled) => enablePopups = enabled;
        public bool IsEnabled() => enablePopups;
        public void SetDamageThreshold(float threshold) => damageThreshold = threshold;
        public void SetMaxActivePopups(int max) => maxActivePopups = max;
    }
}