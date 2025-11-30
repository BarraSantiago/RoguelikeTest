using PlayerDirectory;
using PlayerDirectory.LevelingSystem;
using PowerUps;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View.UI.Leveling
{
    public class ChoiceCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statIncreaseText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject rarityIndicator;

        [Header("Visual Settings")] 
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = new(1f, 1f, 0.8f);
        [SerializeField] private Color banishColor = new(1f, 0.3f, 0.3f);

        private object _choice;
        private Player _player;
        private LevelUpUI _levelUpUI;
        private bool _isBanishHighlighted;

        public void Initialize(object choice, Player player, LevelUpUI levelUpUI)
        {
            _choice = choice;
            _player = player;
            _levelUpUI = levelUpUI;

            SetupCard();
            selectButton.onClick.AddListener(OnSelectClicked);

            if (backgroundImage)
            {
                backgroundImage.color = normalColor;
            }
        }

        private void SetupCard()
        {
            if (_choice is StatUpgradeOption statUpgrade)
            {
                SetupStatUpgrade(statUpgrade);
                ApplyRarityColor(statUpgrade.rarity);
            }
            else if (_choice is PowerUp powerUp)
            {
                SetupPowerUpUpgrade(powerUp);
                ApplyPowerUpColor();
            }
        }
        
        private void ApplyRarityColor(StatRarity rarity)
        {
            if (backgroundImage == null) return;
        
            Color rarityColor = rarity switch
            {
                StatRarity.Common => new Color(0.8f, 0.8f, 0.8f),
                StatRarity.Rare => new Color(0.3f, 0.6f, 1f),
                StatRarity.Epic => new Color(0.7f, 0.3f, 1f),
                StatRarity.Legendary => new Color(1f, 0.6f, 0f),
                _ => Color.white
            };
        
            backgroundImage.color = rarityColor;
            normalColor = rarityColor;
        }
        
        private void ApplyPowerUpColor()
        {
            if (!backgroundImage) return;
        
            Color powerUpColor = new Color(0.4f, 0.8f, 0.4f);
            backgroundImage.color = powerUpColor;
            normalColor = powerUpColor;
        }

        private void SetupStatUpgrade(StatUpgradeOption upgrade)
        {
            titleText.text = upgrade.name;
            descriptionText.text = upgrade.description;
            statIncreaseText.text = GetStatIncreaseText(upgrade);

            if (levelText != null)
            {
                levelText.gameObject.SetActive(false);
            }

            // Set icon based on upgrade type (optional)
            SetIconForStatUpgrade(upgrade);
        }

        private void SetIconForStatUpgrade(StatUpgradeOption upgrade)
        {
            // TODO: Load sprite resources for different upgrade types
            // Example:
            // if (upgrade.name.Contains("Damage"))
            //     iconImage.sprite = Resources.Load<Sprite>("Icons/Damage");
        }

        private void SetupPowerUpUpgrade(PowerUp powerUp)
        {
            titleText.text = powerUp.name;
            descriptionText.text = $"Upgrade {powerUp.name}";

            if (levelText != null)
            {
                levelText.text = $"Lvl {powerUp.Level.level} → {powerUp.Level.level + 1}";
                levelText.gameObject.SetActive(true);
            }

            statIncreaseText.text = GetPowerUpIncreaseText(powerUp);

            // Use power-up icon if available
            if (iconImage != null && powerUp.PowerUpStats != null)
            {
                // TODO: Set icon from PowerUpStats
            }
        }

        private string GetStatIncreaseText(StatUpgradeOption upgrade)
        {
            if (upgrade.name.Contains("Damage"))
                return $"Damage: {_player.statsMod.damageMod:F2} → {_player.statsMod.damageMod * 1.2f:F2}";

            if (upgrade.name.Contains("Attack Speed"))
                return
                    $"Attack Speed: {_player.statsMod.attackSpeedMod:F2} → {_player.statsMod.attackSpeedMod * 0.85f:F2}";

            if (upgrade.name.Contains("Range"))
                return $"Range: {_player.statsMod.rangeMod:F2} → {_player.statsMod.rangeMod * 1.25f:F2}";

            if (upgrade.name.Contains("Critical"))
                return
                    $"Crit: {_player.PlayerStats.CriticalChance:P0} → {(_player.PlayerStats.CriticalChance + 0.05f):P0}";

            if (upgrade.name.Contains("Movement"))
                return $"Speed: {_player.PlayerStats.Speed:F1} → {_player.PlayerStats.Speed * 1.15f:F1}";

            if (upgrade.name.Contains("Health") && !upgrade.name.Contains("Regeneration"))
                return $"HP: {_player.PlayerStats.Health:F0} → {_player.PlayerStats.Health * 1.2f:F0}";

            if (upgrade.name.Contains("Regeneration"))
                return
                    $"Regen: {_player.PlayerStats.HealthRegen + _player.statsMod.healthRegenBonus:F1}/s → {_player.PlayerStats.HealthRegen + _player.statsMod.healthRegenBonus + 1f:F1}/s";

            if (upgrade.name.Contains("Armor"))
                return $"Armor: {_player.statsMod.armorBonus:F0} → {_player.statsMod.armorBonus + 5f:F0}";

            if (upgrade.name.Contains("Cooldown"))
                return
                    $"CDR: {_player.statsMod.cooldownReductionBonus:P0} → {_player.statsMod.cooldownReductionBonus + 0.1f:P0}";

            if (upgrade.name.Contains("Pickup"))
                return
                    $"Radius: {_player.PlayerStats.PickupRadius * _player.statsMod.pickupRadiusMod:F1} → {_player.PlayerStats.PickupRadius * _player.statsMod.pickupRadiusMod * 1.25f:F1}";

            if (upgrade.name.Contains("Magnetism"))
                return $"Magnet: {_player.statsMod.magnetismMod:F2} → {_player.statsMod.magnetismMod * 1.3f:F2}";

            if (upgrade.name.Contains("Experience"))
                return $"XP: {_player.statsMod.growthMod:P0} → {_player.statsMod.growthMod * 1.2f:P0}";

            if (upgrade.name.Contains("Luck"))
                return $"Luck: {_player.statsMod.luckMod:F2} → {_player.statsMod.luckMod * 1.15f:F2}";

            if (upgrade.name.Contains("Projectile"))
                return
                    $"Projectiles: {_player.PlayerStats.ProjectileCount + _player.statsMod.projectileCountBonus} → {_player.PlayerStats.ProjectileCount + _player.statsMod.projectileCountBonus + 1}";

            if (upgrade.name.Contains("Pierce"))
                return
                    $"Pierce: {_player.PlayerStats.PierceCount + _player.statsMod.pierceCountBonus} → {_player.PlayerStats.PierceCount + _player.statsMod.pierceCountBonus + 1}";

            if (upgrade.name.Contains("Revival"))
                return $"Revivals: {_player.GetRevivals()} → {_player.GetRevivals() + 1}";

            return "";
        }

        private string GetPowerUpIncreaseText(PowerUp powerUp)
        {
            float currentDamage = powerUp.PowerUpStats.Damage;
            float nextDamage = currentDamage * (1 + powerUp.Level.damageIncrease);

            return $"Damage: {currentDamage:F1} → {nextDamage:F1}";
        }

        public void SetBanishHighlight(bool highlighted)
        {
            _isBanishHighlighted = highlighted;

            if (backgroundImage)
            {
                backgroundImage.color = highlighted ? banishColor : normalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (backgroundImage && !_isBanishHighlighted)
            {
                backgroundImage.color = hoverColor;
            }

            string tooltipText = GenerateTooltipText();
            _levelUpUI.ShowTooltip(tooltipText, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (backgroundImage && !_isBanishHighlighted)
            {
                backgroundImage.color = normalColor;
            }

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
            upgrade.ApplyUpgrade(_player);
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
                   $"Damage: {powerUp.PowerUpStats.Damage:F1} → " +
                   $"{powerUp.PowerUpStats.Damage * (1 + powerUp.Level.damageIncrease):F1}\n" +
                   $"Attack Speed: {powerUp.PowerUpStats.AttackSpeed:F2}s → " +
                   $"{powerUp.PowerUpStats.AttackSpeed * (1 - powerUp.Level.attackSpeedDecrease):F2}s\n" +
                   $"Range: {powerUp.PowerUpStats.Range:F1} → " +
                   $"{powerUp.PowerUpStats.Range * (1 + powerUp.Level.rangeIncrease):F1}";
        }

        private float CalculateCurrentDPS()
        {
            float baseDamage = _player.PlayerStats.Damage * _player.statsMod.damageMod;
            float attacksPerSecond = 1f / (_player.PlayerStats.AttackSpeed * _player.statsMod.attackSpeedMod);

            float critMultiplier = 1f + (_player.PlayerStats.CriticalChance *
                                         (_player.PlayerStats.CriticalDamage - 1f));

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
                _player.PlayerStats.CriticalChance -= 0.05f;
            else if (upgrade.name.Contains("Movement"))
                _player.PlayerStats.Speed /= 1.15f;
            else if (upgrade.name.Contains("Health") && !upgrade.name.Contains("Regeneration"))
            {
                _player.PlayerStats.Health /= 1.2f;
                _player.PlayerStats.MaxHealth = _player.PlayerStats.Health;
            }
            else if (upgrade.name.Contains("Regeneration"))
                _player.statsMod.healthRegenBonus -= 1f;
            else if (upgrade.name.Contains("Armor"))
                _player.statsMod.armorBonus -= 5f;
            else if (upgrade.name.Contains("Cooldown"))
                _player.statsMod.cooldownReductionBonus -= 0.1f;
            else if (upgrade.name.Contains("Pickup"))
                _player.statsMod.pickupRadiusMod /= 1.25f;
            else if (upgrade.name.Contains("Magnetism"))
                _player.statsMod.magnetismMod /= 1.3f;
            else if (upgrade.name.Contains("Experience"))
                _player.statsMod.growthMod /= 1.2f;
            else if (upgrade.name.Contains("Luck"))
                _player.statsMod.luckMod /= 1.15f;
            else if (upgrade.name.Contains("Projectile"))
                _player.statsMod.projectileCountBonus--;
            else if (upgrade.name.Contains("Pierce"))
                _player.statsMod.pierceCountBonus--;
            else if (upgrade.name.Contains("Revival"))
                _player.statsMod.revivalBonus--;
        }

        private void OnSelectClicked()
        {
            _levelUpUI.OnChoiceSelected(_choice);
        }

        public object GetChoice() => _choice;
    }
}