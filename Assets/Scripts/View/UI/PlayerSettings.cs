using UnityEngine;
using UnityEngine.UI;
using PlayerDirectory;

namespace View.UI
{
    public class PlayerSettings : MonoBehaviour
    {
        [SerializeField] private Toggle autoTargetToggle;
        [SerializeField] private Toggle autoAttackToggle;
        [SerializeField] private Player player;

        private void Start()
        {
            if (autoTargetToggle)
            {
                autoTargetToggle.isOn = player.AutoTargetEnabled;
                autoTargetToggle.onValueChanged.AddListener(OnAutoTargetToggled);
            }

            if (!autoAttackToggle) return;
            autoAttackToggle.isOn = player.AutoAttackEnabled;
            autoAttackToggle.onValueChanged.AddListener(OnAutoAttackToggled);
        }

        private void OnAutoTargetToggled(bool value)
        {
            if (player)
            {
                player.AutoTargetEnabled = value;
            }
        }

        private void OnAutoAttackToggled(bool value)
        {
            if (player)
            {
                player.AutoAttackEnabled = value;
            }
        }

        private void OnDestroy()
        {
            autoTargetToggle?.onValueChanged.RemoveListener(OnAutoTargetToggled);
            autoAttackToggle?.onValueChanged.RemoveListener(OnAutoAttackToggled);
        }
    }
}