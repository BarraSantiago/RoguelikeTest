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
            if (iconImage != null && powerUp.PowerUpBaseStats != null)
            {
                // TODO: Set icon from PowerUpStats
            }
        }

        private string GetStatIncreaseText(StatUpgradeOption upgrade)
        {
            if (upgrade.name.Contains("Damage"))
                return $"Damage: {_player.baseStatsMod.damageMod:F2} → {_player.baseStatsMod.damageMod * 1.2f:F2}";

            if (upgrade.name.Contains("Attack Speed"))
                return
                    $"Attack Speed: {_player.baseStatsMod.attackSpeedMod:F2} → {_player.baseStatsMod.attackSpeedMod * 0.85f:F2}";

            if (upgrade.name.Contains("Range"))
                return $"Range: {_player.baseStatsMod.rangeMod:F2} → {_player.baseStatsMod.rangeMod * 1.25f:F2}";

            if (upgrade.name.Contains("Critical"))
                return
                    $"Crit: {_player.PlayerBaseStats.CriticalChance:P0} → {(_player.PlayerBaseStats.CriticalChance + 0.05f):P0}";

            if (upgrade.name.Contains("Movement"))
                return $"Speed: {_player.PlayerBaseStats.Speed:F1} → {_player.PlayerBaseStats.Speed * 1.15f:F1}";

            if (upgrade.name.Contains("Health") && !upgrade.name.Contains("Regeneration"))
                return $"HP: {_player.PlayerBaseStats.Health:F0} → {_player.PlayerBaseStats.Health * 1.2f:F0}";

            if (upgrade.name.Contains("Regeneration"))
                return
                    $"Regen: {_player.PlayerBaseStats.HealthRegen + _player.baseStatsMod.healthRegenBonus:F1}/s → {_player.PlayerBaseStats.HealthRegen + _player.baseStatsMod.healthRegenBonus + 1f:F1}/s";

            if (upgrade.name.Contains("Armor"))
                return $"Armor: {_player.baseStatsMod.armorBonus:F0} → {_player.baseStatsMod.armorBonus + 5f:F0}";

            if (upgrade.name.Contains("Cooldown"))
                return
                    $"CDR: {_player.baseStatsMod.cooldownReductionBonus:P0} → {_player.baseStatsMod.cooldownReductionBonus + 0.1f:P0}";

            if (upgrade.name.Contains("Pickup"))
                return
                    $"Radius: {_player.PlayerBaseStats.PickupRadius * _player.baseStatsMod.pickupRadiusMod:F1} → {_player.PlayerBaseStats.PickupRadius * _player.baseStatsMod.pickupRadiusMod * 1.25f:F1}";

            if (upgrade.name.Contains("Magnetism"))
                return $"Magnet: {_player.baseStatsMod.magnetismMod:F2} → {_player.baseStatsMod.magnetismMod * 1.3f:F2}";

            if (upgrade.name.Contains("Experience"))
                return $"XP: {_player.baseStatsMod.growthMod:P0} → {_player.baseStatsMod.growthMod * 1.2f:P0}";

            if (upgrade.name.Contains("Luck"))
                return $"Luck: {_player.baseStatsMod.luckMod:F2} → {_player.baseStatsMod.luckMod * 1.15f:F2}";

            if (upgrade.name.Contains("Projectile"))
                return
                    $"Projectiles: {_player.PlayerBaseStats.ProjectileCount + _player.baseStatsMod.projectileCountBonus} → {_player.PlayerBaseStats.ProjectileCount + _player.baseStatsMod.projectileCountBonus + 1}";

            if (upgrade.name.Contains("Pierce"))
                return
                    $"Pierce: {_player.PlayerBaseStats.PierceCount + _player.baseStatsMod.pierceCountBonus} → {_player.PlayerBaseStats.PierceCount + _player.baseStatsMod.pierceCountBonus + 1}";

            if (upgrade.name.Contains("Revival"))
                return $"Revivals: {_player.GetRevivals()} → {_player.GetRevivals() + 1}";

            return "";
        }

        private string GetPowerUpIncreaseText(PowerUp powerUp)
        {
            float currentDamage = powerUp.PowerUpBaseStats.Damage;
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
                   $"Damage: {powerUp.PowerUpBaseStats.Damage:F1} → " +
                   $"{powerUp.PowerUpBaseStats.Damage * (1 + powerUp.Level.damageIncrease):F1}\n" +
                   $"Attack Speed: {powerUp.PowerUpBaseStats.AttackSpeed:F2}s → " +
                   $"{powerUp.PowerUpBaseStats.AttackSpeed * (1 - powerUp.Level.attackSpeedDecrease):F2}s\n" +
                   $"Range: {powerUp.PowerUpBaseStats.Range:F1} → " +
                   $"{powerUp.PowerUpBaseStats.Range * (1 + powerUp.Level.rangeIncrease):F1}";
        }

        private float CalculateCurrentDPS()
        {
            float baseDamage = _player.PlayerBaseStats.Damage * _player.baseStatsMod.damageMod;
            float attacksPerSecond = 1f / (_player.PlayerBaseStats.AttackSpeed * _player.baseStatsMod.attackSpeedMod);

            float critMultiplier = 1f + (_player.PlayerBaseStats.CriticalChance *
                                         (_player.PlayerBaseStats.CriticalDamage - 1f));

            return baseDamage * attacksPerSecond * critMultiplier;
        }

        private void RevertStatUpgrade(StatUpgradeOption upgrade)
        {
            if (upgrade.name.Contains("Damage"))
                _player.baseStatsMod.damageMod /= 1.2f;
            else if (upgrade.name.Contains("Attack Speed"))
                _player.baseStatsMod.attackSpeedMod /= 0.85f;
            else if (upgrade.name.Contains("Range"))
                _player.baseStatsMod.rangeMod /= 1.25f;
            else if (upgrade.name.Contains("Critical"))
                _player.PlayerBaseStats.CriticalChance -= 0.05f;
            else if (upgrade.name.Contains("Movement"))
                _player.PlayerBaseStats.Speed /= 1.15f;
            else if (upgrade.name.Contains("Health") && !upgrade.name.Contains("Regeneration"))
            {
                _player.PlayerBaseStats.Health /= 1.2f;
                _player.PlayerBaseStats.MaxHealth = _player.PlayerBaseStats.Health;
            }
            else if (upgrade.name.Contains("Regeneration"))
                _player.baseStatsMod.healthRegenBonus -= 1f;
            else if (upgrade.name.Contains("Armor"))
                _player.baseStatsMod.armorBonus -= 5f;
            else if (upgrade.name.Contains("Cooldown"))
                _player.baseStatsMod.cooldownReductionBonus -= 0.1f;
            else if (upgrade.name.Contains("Pickup"))
                _player.baseStatsMod.pickupRadiusMod /= 1.25f;
            else if (upgrade.name.Contains("Magnetism"))
                _player.baseStatsMod.magnetismMod /= 1.3f;
            else if (upgrade.name.Contains("Experience"))
                _player.baseStatsMod.growthMod /= 1.2f;
            else if (upgrade.name.Contains("Luck"))
                _player.baseStatsMod.luckMod /= 1.15f;
            else if (upgrade.name.Contains("Projectile"))
                _player.baseStatsMod.projectileCountBonus--;
            else if (upgrade.name.Contains("Pierce"))
                _player.baseStatsMod.pierceCountBonus--;
            else if (upgrade.name.Contains("Revival"))
                _player.baseStatsMod.revivalBonus--;
        }

        private void OnSelectClicked()
        {
            _levelUpUI.OnChoiceSelected(_choice);
        }

        public object GetChoice() => _choice;
    }
}