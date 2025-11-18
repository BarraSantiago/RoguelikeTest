using PlayerDirectory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
        using TMPro;
#endif

namespace View.UI
{
    public class PlayerHealthSlider : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Player player;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image fillImage;

        [Header("Options")] 
        [SerializeField] private bool smooth = true;
        [SerializeField] private float smoothSpeed = 6f;
        [SerializeField] private bool showNumeric = true;
        [SerializeField] private Gradient healthGradient;

        private float _currentValue;

        private void Awake()
        {
            if (healthSlider == null)
            {
                Debug.LogWarning("PlayerHealthSlider: healthSlider not assigned.");
                enabled = false;
                return;
            }

            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;

            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }

            _currentValue = healthSlider.value;
        }

        private void LateUpdate()
        {
            if (!player) return;

            float target = Mathf.Clamp01(player.GetHealthPercentage());

            if (smooth)
            {
                _currentValue = Mathf.Lerp(_currentValue, target, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
            }
            else
            {
                _currentValue = target;
            }

            healthSlider.value = _currentValue;

            if (fillImage && healthGradient != null)
            {
                fillImage.color = healthGradient.Evaluate(_currentValue);
            }

            if (showNumeric && healthText)
            {
                float currentHp = player.GetCurrentHealth();
                float maxHp = player.GetMaxHealth();
                healthText.text = $"{Mathf.RoundToInt(currentHp)}/{Mathf.RoundToInt(maxHp)}";
            }
        }

        // Optional runtime binding
        public void Bind(Player target)
        {
            player = target;
            if (player != null)
            {
                healthSlider.value = Mathf.Clamp01(player.GetHealthPercentage());
                _currentValue = healthSlider.value;
            }
        }
    }
}