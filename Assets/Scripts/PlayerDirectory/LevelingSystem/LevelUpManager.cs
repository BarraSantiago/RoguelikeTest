using System;
using System.Collections.Generic;
using ClassesDirectory;
using PowerUps;
using UnityEngine;
using Utilities;

namespace PlayerDirectory.LevelingSystem
{
    public enum StatRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [Serializable]
    public class StatUpgradeOption
    {
        public string name;
        public string description;
        public Action<Player> ApplyUpgrade;
        public StatRarity rarity;

        public Color GetRarityColor()
        {
            return rarity switch
            {
                StatRarity.Common => new Color(0.8f, 0.8f, 0.8f), // Light gray
                StatRarity.Rare => new Color(0.3f, 0.6f, 1f), // Blue
                StatRarity.Epic => new Color(0.7f, 0.3f, 1f), // Purple
                StatRarity.Legendary => new Color(1f, 0.6f, 0f), // Orange/Gold
                _ => Color.white
            };
        }

        public float GetRarityMultiplier()
        {
            return rarity switch
            {
                StatRarity.Common => 1f,
                StatRarity.Rare => 1.5f,
                StatRarity.Epic => 2f,
                StatRarity.Legendary => 3f,
                _ => 1f
            };
        }
    }

    public class LevelUpManager : MonoBehaviour
    {
        [SerializeField] private int numberOfChoices = 3;
        [SerializeField] private float commonChance = 0.65f;
        [SerializeField] private float rareChance = 0.25f;
        [SerializeField] private float epicChance = 0.08f;
        [SerializeField] private float legendaryChance = 0.02f;

        private List<StatUpgradeOption> _availableStatUpgrades;
        private HashSet<object> _banishedChoices = new();

        private void Awake()
        {
            InitializeStatUpgrades();
        }

        private void InitializeStatUpgrades()
        {
            _availableStatUpgrades = new List<StatUpgradeOption>();
        }

        private StatRarity RollRarity(float luck)
        {
            // Luck affects legendary chance directly
            // Base 2% legendary, each point of luck adds 0.5% (so 1.0 luck = 2.5% legendary)
            float legendaryBonus = (luck - 1f) * 0.005f;
            float adjustedLegendary = legendaryChance + legendaryBonus;
        
            if (CombatUtilities.RollLuck(adjustedLegendary * 100f, 1f)) return StatRarity.Legendary;
            if (CombatUtilities.RollLuck(epicChance * 100f, 1f)) return StatRarity.Epic;
            if (CombatUtilities.RollLuck(rareChance * 100f, 1f)) return StatRarity.Rare;
            return StatRarity.Common;
        }
        
        private StatUpgradeOption CreateStatUpgrade(string baseName, string baseDescription,
            Action<Player, float> applyUpgrade, StatRarity rarity, Func<Player, float, string> getDescription = null)
        {
            float multiplier = rarity switch
            {
                StatRarity.Common => 1f,
                StatRarity.Rare => 1.5f,
                StatRarity.Epic => 2f,
                StatRarity.Legendary => 3f,
                _ => 1f
            };
        
            string finalDescription = getDescription != null 
                ? getDescription(null, multiplier) 
                : baseDescription;
        
            return new StatUpgradeOption
            {
                name = $"{baseName}",
                description = $"{finalDescription} ({rarity})",
                rarity = rarity,
                ApplyUpgrade = player => applyUpgrade(player, multiplier)
            };
        }
        

        private StatUpgradeOption CreateStatUpgrade(string baseName, string baseDescription,
            Action<Player, float> applyUpgrade, StatRarity rarity)
        {
            float multiplier = rarity switch
            {
                StatRarity.Common => 1f,
                StatRarity.Rare => 1.5f,
                StatRarity.Epic => 2f,
                StatRarity.Legendary => 3f,
                _ => 1f
            };

            return new StatUpgradeOption
            {
                name = $"{baseName}",
                description = $"{baseDescription} ({rarity})",
                rarity = rarity,
                ApplyUpgrade = player => applyUpgrade(player, multiplier)
            };
        }

        public List<object> GenerateLevelUpChoices(Player player)
        {
            List<object> choices = new List<object>();
            List<PowerUp> upgradablePowerUps = player.powerUps.FindAll(p => p.CanUpgrade());

            upgradablePowerUps.RemoveAll(powerUp => _banishedChoices.Contains(powerUp));

            int totalAvailable = upgradablePowerUps.Count;
            int actualChoices = Mathf.Min(numberOfChoices, totalAvailable + 100); // Always have stat options

            float luckMod = player.baseStatsMod.luckMod;

            for (int i = 0; i < actualChoices; i++)
            {
                bool choosePowerUp = upgradablePowerUps.Count > 0 && UnityEngine.Random.value > 0.7f;

                if (choosePowerUp)
                {
                    int randomIndex = UnityEngine.Random.Range(0, upgradablePowerUps.Count);
                    choices.Add(upgradablePowerUps[randomIndex]);
                    upgradablePowerUps.RemoveAt(randomIndex);
                }
                else
                {
                    StatUpgradeOption statOption = GenerateRandomStatUpgrade(player, player.PlayerBaseStats.Luck * luckMod);
                    if (statOption != null && !_banishedChoices.Contains(statOption))
                    {
                        choices.Add(statOption);
                    }
                }
            }

            return choices;
        }

        private StatUpgradeOption GenerateRandomStatUpgrade(Player player, float luckMod)
        {
            List<StatUpgradeOption> availableStats = GetClassFilteredStats(player, luckMod);

            if (availableStats.Count == 0) return null;

            int randomIndex = UnityEngine.Random.Range(0, availableStats.Count);
            return availableStats[randomIndex];
        }

        private List<StatUpgradeOption> GetClassFilteredStats(Player player, float luck)
        {
            List<StatUpgradeOption> filtered = new List<StatUpgradeOption>();
            StatRarity rarity = RollRarity(luck);
        
            Classes classType = player.GetPlayerClassType();
        
            // Common stats for all classes
            filtered.Add(CreateStatUpgrade("Increased Damage", "+20% Damage",
                (p, mult) => p.baseStatsMod.damageMod *= 1f + (0.2f * mult), rarity,
                (p, mult) => $"+{20f * mult:F0}% Damage"));
        
            filtered.Add(CreateStatUpgrade("Attack Speed", "+15% Attack Speed",
                (p, mult) => p.baseStatsMod.attackSpeedMod *= 1f - (0.15f * mult), rarity,
                (p, mult) => $"+{15f * mult:F0}% Attack Speed"));
        
            filtered.Add(CreateStatUpgrade("Increased Range", "+25% Range",
                (p, mult) => p.baseStatsMod.rangeMod *= 1f + (0.25f * mult), rarity,
                (p, mult) => $"+{25f * mult:F0}% Range"));
        
            filtered.Add(CreateStatUpgrade("Critical Chance", "+5% Critical Chance",
                (p, mult) => p.PlayerBaseStats.CriticalChance += 0.05f * mult, rarity,
                (p, mult) => $"+{5f * mult:F0}% Critical Chance"));
        
            filtered.Add(CreateStatUpgrade("Movement Speed", "+15% Speed",
                (p, mult) => p.PlayerBaseStats.Speed *= 1f + (0.15f * mult), rarity,
                (p, mult) => $"+{15f * mult:F0}% Movement Speed"));
        
            filtered.Add(CreateStatUpgrade("Max Health", "+20% Health",
                (p, mult) =>
                {
                    p.PlayerBaseStats.Health *= 1f + (0.2f * mult);
                    p.PlayerBaseStats.MaxHealth = p.PlayerBaseStats.Health;
                }, rarity,
                (p, mult) => $"+{20f * mult:F0}% Max Health"));
        
            filtered.Add(CreateStatUpgrade("Health Regeneration", "+1 HP/s Regen",
                (p, mult) => p.baseStatsMod.healthRegenBonus += 1f * mult, rarity,
                (p, mult) => $"+{1f * mult:F1} HP/s Regeneration"));
        
            filtered.Add(CreateStatUpgrade("Armor", "+5 Armor",
                (p, mult) => p.baseStatsMod.armorBonus += 5f * mult, rarity,
                (p, mult) => $"+{5f * mult:F0} Armor"));
        
            filtered.Add(CreateStatUpgrade("Cooldown Reduction", "+10% CDR",
                (p, mult) => p.baseStatsMod.cooldownReductionBonus += 0.1f * mult, rarity,
                (p, mult) => $"+{10f * mult:F0}% Cooldown Reduction"));
        
            filtered.Add(CreateStatUpgrade("Pickup Radius", "+25% Pickup Range",
                (p, mult) => p.baseStatsMod.pickupRadiusMod *= 1f + (0.25f * mult), rarity,
                (p, mult) => $"+{25f * mult:F0}% Pickup Radius"));
        
            filtered.Add(CreateStatUpgrade("Magnetism", "+30% Attraction Speed",
                (p, mult) => p.baseStatsMod.magnetismMod *= 1f + (0.3f * mult), rarity,
                (p, mult) => $"+{30f * mult:F0}% Magnetism"));
        
            filtered.Add(CreateStatUpgrade("Experience Gain", "+20% XP",
                (p, mult) => p.baseStatsMod.growthMod *= 1f + (0.2f * mult), rarity,
                (p, mult) => $"+{20f * mult:F0}% Experience Gain"));
        
            filtered.Add(CreateStatUpgrade("Luck", "+15% Luck",
                (p, mult) => p.baseStatsMod.luckMod *= 1f + (0.15f * mult), rarity,
                (p, mult) => $"+{15f * mult:F0}% Luck"));
        
            if (rarity == StatRarity.Legendary)
            {
                filtered.Add(CreateStatUpgrade("Revival", "+1 Extra Life",
                    (p, mult) => p.baseStatsMod.revivalBonus += Mathf.RoundToInt(mult), rarity,
                    (p, mult) => $"+{Mathf.RoundToInt(mult)} Revival"));
            }
        
            // Class-specific stats
            if (classType == Classes.Warrior)
            {
                filtered.Add(CreateStatUpgrade("Increased Area", "+25% Attack Area",
                    (p, mult) => p.baseStatsMod.areaMod *= 1f + (0.25f * mult), rarity,
                    (p, mult) => $"+{25f * mult:F0}% Area"));
        
                filtered.Add(CreateStatUpgrade("Attack Count", "+1 Multi-Attack",
                    (p, mult) => p.baseStatsMod.projectileCountBonus += Mathf.RoundToInt(mult), rarity,
                    (p, mult) => $"+{Mathf.RoundToInt(mult)} Attack Count"));
        
                filtered.Add(CreateStatUpgrade("Knockback", "+25% Knockback",
                    (p, mult) => p.baseStatsMod.knockbackMod *= 1f + (0.25f * mult), rarity,
                    (p, mult) => $"+{25f * mult:F0}% Knockback"));
            }
            else if (classType == Classes.Ranger)
            {
                filtered.Add(CreateStatUpgrade("Projectile Count", "+1 Projectile",
                    (p, mult) => p.baseStatsMod.projectileCountBonus += Mathf.RoundToInt(mult), rarity,
                    (p, mult) => $"+{Mathf.RoundToInt(mult)} Projectile"));
        
                filtered.Add(CreateStatUpgrade("Pierce", "+1 Pierce",
                    (p, mult) => p.baseStatsMod.pierceCountBonus += Mathf.RoundToInt(mult), rarity,
                    (p, mult) => $"+{Mathf.RoundToInt(mult)} Pierce"));
        
                filtered.Add(CreateStatUpgrade("Projectile Speed", "+20% Projectile Speed",
                    (p, mult) => p.baseStatsMod.projectileSpeedMod *= 1f + (0.2f * mult), rarity,
                    (p, mult) => $"+{20f * mult:F0}% Projectile Speed"));
            }
        
            return filtered;
        }

        public void ApplyChoice(Player player, object choice)
        {
            if (choice is StatUpgradeOption statUpgrade)
            {
                statUpgrade.ApplyUpgrade(player);
                Debug.Log($"Applied {statUpgrade.rarity} stat upgrade: {statUpgrade.name}");
            }
            else if (choice is PowerUp powerUp)
            {
                powerUp.Upgrade();
                Debug.Log($"Upgraded power-up: {powerUp.name}");
            }
        }

        public void BanishChoice(object choice)
        {
            _banishedChoices.Add(choice);

            string choiceName = choice switch
            {
                StatUpgradeOption stat => stat.name,
                PowerUp powerUp => powerUp.name,
                _ => "Unknown"
            };

            Debug.Log($"Banished choice: {choiceName}");
        }

        public void ResetBanishedChoices()
        {
            _banishedChoices.Clear();
            Debug.Log("Banished choices reset");
        }
    }
}