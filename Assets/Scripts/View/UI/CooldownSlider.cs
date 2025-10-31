using UnityEngine;
using UnityEngine.UI;
using PlayerDirectory;

namespace View.UI
{
    public class CooldownSlider : MonoBehaviour
    {
        [SerializeField] private Slider defenseSlider;
        [SerializeField] private Slider specialSlider;
        [SerializeField] private Player player;

        private void Update()
        {
            if (!player || !defenseSlider) return;

            defenseSlider.value = player.DefenseCooldownProgress;
            specialSlider.value = player.SpecialAttackCooldownProgress;
        }
    }
}