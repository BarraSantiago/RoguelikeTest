using PlayerDirectory;
using PowerUps;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View.UI.Leveling
{
    public class ChoiceCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statIncreaseText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button selectButton;

        private object _choice;
        private Player _player;
        private LevelUpUI _levelUpUI;

        public void Initialize(object choice, Player player, LevelUpUI levelUpUI)
        {
            _choice = choice;
            _player = player;
            _levelUpUI = levelUpUI;

            SetupCard();
            selectButton.onClick.AddListener(OnSelectClicked);
        }

        private void SetupCard()
        {
            if (_choice is StatUpgradeOption statUpgrade)
            {
                SetupStatUpgrade(statUpgrade);
            }
            else if (_choice is PowerUp powerUp)
            {
                SetupPowerUpUpgrade(powerUp);
            }
        }

        private void SetupStatUpgrade(StatUpgradeOption upgrade)
        {
            titleText.text = upgrade.name;
            descriptionText.text = upgrade.description;
            statIncreaseText.text = GetStatIncreaseText(upgrade);
            levelText.gameObject.SetActive(false);
        }

        private void SetupPowerUpUpgrade(PowerUp powerUp)
        {
            titleText.text = powerUp.name;
            descriptionText.text = $"Upgrade {powerUp.name}";
            levelText.text = $"Lvl {powerUp.Level.level} → {powerUp.Level.level + 1}";
            levelText.gameObject.SetActive(true);
            
            statIncreaseText.text = GetPowerUpIncreaseText(powerUp);
        }

        private string GetStatIncreaseText(StatUpgradeOption upgrade)
        {
            if (upgrade.name.Contains("Damage"))
                return $"Damage: {_player.statsMod.damageMod:F2} → {_player.statsMod.damageMod * 1.2f:F2}";
            
            if (upgrade.name.Contains("Attack Speed"))
                return $"Attack Speed: {_player.statsMod.attackSpeedMod:F2} → {_player.statsMod.attackSpeedMod * 0.85f:F2}";
            
            if (upgrade.name.Contains("Range"))
                return $"Range: {_player.statsMod.rangeMod:F2} → {_player.statsMod.rangeMod * 1.25f:F2}";
            
            if (upgrade.name.Contains("Critical"))
                return $"Crit: {_player.playerStats.criticalChance:P0} → {(_player.playerStats.criticalChance + 0.05f):P0}";
            
            if (upgrade.name.Contains("Movement"))
                return $"Speed: {_player.playerStats.speed:F1} → {_player.playerStats.speed * 1.15f:F1}";
            
            if (upgrade.name.Contains("Health"))
                return $"Health: {_player.playerStats.health:F0} → {_player.playerStats.health * 1.2f:F0}";

            return "";
        }

        private string GetPowerUpIncreaseText(PowerUp powerUp)
        {
            float currentDamage = powerUp.PowerUpStats.damage;
            float nextDamage = currentDamage * (1 + powerUp.Level.damageIncrease);
            
            return $"Damage: {currentDamage:F1} → {nextDamage:F1}";
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            string tooltipText = GenerateTooltipText();
            _levelUpUI.ShowTooltip(tooltipText, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _levelUpUI.HideTooltip();
        }

        private string GenerateTooltipText()
        {
            if (_choice is StatUpgradeOption statUpgrade)
            {
                return GenerateStatTooltip(statUpgrade);
            }
            else if (_choice is PowerUp powerUp)
            {
                return GeneratePowerUpTooltip(powerUp);
            }

            return "";
        }

        private string GenerateStatTooltip(StatUpgradeOption upgrade)
        {
            float currentDPS = CalculateCurrentDPS();
            
            // Temporarily apply upgrade to calculate new DPS
            upgrade.applyUpgrade(_player);
            float newDPS = CalculateCurrentDPS();
            
            // Revert the upgrade
            RevertStatUpgrade(upgrade);

            float dpsIncrease = ((newDPS - currentDPS) / currentDPS) * 100f;

            return $"<b>{upgrade.name}</b>\n\n" +
                   $"{upgrade.description}\n\n" +
                   $"Current DPS: {currentDPS:F1}\n" +
                   $"New DPS: {newDPS:F1}\n" +
                   $"DPS Increase: <color=green>+{dpsIncrease:F1}%</color>";
        }

        private string GeneratePowerUpTooltip(PowerUp powerUp)
        {
            return $"<b>{powerUp.name} - Level {powerUp.Level.level + 1}</b>\n\n" +
                   $"Damage: {powerUp.PowerUpStats.damage:F1} → " +
                   $"{powerUp.PowerUpStats.damage * (1 + powerUp.Level.damageIncrease):F1}\n" +
                   $"Attack Speed: {powerUp.PowerUpStats.attackSpeed:F2}s → " +
                   $"{powerUp.PowerUpStats.attackSpeed * (1 - powerUp.Level.attackSpeedDecrease):F2}s\n" +
                   $"Range: {powerUp.PowerUpStats.range:F1} → " +
                   $"{powerUp.PowerUpStats.range * (1 + powerUp.Level.rangeIncrease):F1}";
        }

        private float CalculateCurrentDPS()
        {
            float baseDamage = _player.playerStats.damage * _player.statsMod.damageMod;
            float attacksPerSecond = 1f / (_player.playerStats.attackSpeed * _player.statsMod.attackSpeedMod);
            
            float critMultiplier = 1f + (_player.playerStats.criticalChance * 
                                   (_player.playerStats.criticalDamage - 1f));
            
            return baseDamage * attacksPerSecond * critMultiplier;
        }

        private void RevertStatUpgrade(StatUpgradeOption upgrade)
        {
            if (upgrade.name.Contains("Damage"))
                _player.statsMod.damageMod /= 1.2f;
            else if (upgrade.name.Contains("Attack Speed"))
                _player.statsMod.attackSpeedMod /= 0.85f;
            else if (upgrade.name.Contains("Range"))
                _player.statsMod.rangeMod /= 1.25f;
            else if (upgrade.name.Contains("Critical"))
                _player.playerStats.criticalChance -= 0.05f;
            else if (upgrade.name.Contains("Movement"))
                _player.playerStats.speed /= 1.15f;
            else if (upgrade.name.Contains("Health"))
                _player.playerStats.health /= 1.2f;
        }

        private void OnSelectClicked()
        {
            _levelUpUI.OnChoiceSelected(_choice);
        }
    }
}