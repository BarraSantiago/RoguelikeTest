using System;
using System.Collections.Generic;
using PowerUps;
using UnityEngine;

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
        public Action<Player> applyUpgrade;
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

        private StatRarity RollRarity(float luckMod)
        {
            float roll = UnityEngine.Random.value / luckMod;

            if (roll < legendaryChance) return StatRarity.Legendary;
            if (roll < legendaryChance + epicChance) return StatRarity.Epic;
            if (roll < legendaryChance + epicChance + rareChance) return StatRarity.Rare;
            return StatRarity.Common;
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
                applyUpgrade = player => applyUpgrade(player, multiplier)
            };
        }

        public List<object> GenerateLevelUpChoices(Player player)
        {
            List<object> choices = new List<object>();
            List<PowerUp> upgradablePowerUps = player.powerUps.FindAll(p => p.CanUpgrade());

            upgradablePowerUps.RemoveAll(powerUp => _banishedChoices.Contains(powerUp));

            int totalAvailable = upgradablePowerUps.Count;
            int actualChoices = Mathf.Min(numberOfChoices, totalAvailable + 100); // Always have stat options

            float luckMod = player.statsMod.luckMod;

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
                    StatUpgradeOption statOption = GenerateRandomStatUpgrade(player, luckMod);
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

        private List<StatUpgradeOption> GetClassFilteredStats(Player player, float luckMod)
        {
            List<StatUpgradeOption> filtered = new List<StatUpgradeOption>();
            StatRarity rarity = RollRarity(luckMod);

            Classes classType = player.GetPlayerClassType();

            // Common stats for all classes
            filtered.Add(CreateStatUpgrade("Increased Damage", "+20% Damage",
                (p, mult) => p.statsMod.damageMod *= 1f + (0.2f * mult), rarity));

            filtered.Add(CreateStatUpgrade("Attack Speed", "+15% Attack Speed",
                (p, mult) => p.statsMod.attackSpeedMod *= 1f - (0.15f * mult), rarity));

            filtered.Add(CreateStatUpgrade("Increased Range", "+25% Range",
                (p, mult) => p.statsMod.rangeMod *= 1f + (0.25f * mult), rarity));

            filtered.Add(CreateStatUpgrade("Critical Chance", "+5% Critical Chance",
                (p, mult) => p.playerStats.criticalChance += 0.05f * mult, rarity));

            filtered.Add(CreateStatUpgrade("Movement Speed", "+15% Speed",
                (p, mult) => p.playerStats.speed *= 1f + (0.15f * mult), rarity));

            filtered.Add(CreateStatUpgrade("Max Health", "+20% Health",
                (p, mult) =>
                {
                    p.playerStats.health *= 1f + (0.2f * mult);
                    p.playerStats.maxHealth = p.playerStats.health;
                }, rarity));

            filtered.Add(CreateStatUpgrade("Health Regeneration", "+1 HP/s Regen",
                (p, mult) => p.statsMod.healthRegenBonus += 1f * mult, rarity));

            filtered.Add(CreateStatUpgrade("Armor", "+5 Armor",
                (p, mult) => p.statsMod.armorBonus += 5f * mult, rarity));

            filtered.Add(CreateStatUpgrade("Cooldown Reduction", "+10% CDR",
                (p, mult) => p.statsMod.cooldownReductionBonus += 0.1f * mult, rarity));

            filtered.Add(CreateStatUpgrade("Pickup Radius", "+25% Pickup Range",
                (p, mult) => p.statsMod.pickupRadiusMod *= 1f + (0.25f * mult), rarity));

            filtered.Add(CreateStatUpgrade("Magnetism", "+30% Attraction Speed",
                (p, mult) => p.statsMod.magnetismMod *= 1f + (0.3f * mult), rarity));

            filtered.Add(CreateStatUpgrade("Experience Gain", "+20% XP",
                (p, mult) => p.statsMod.growthMod *= 1f + (0.2f * mult), rarity));

            filtered.Add(CreateStatUpgrade("Luck", "+15% Luck",
                (p, mult) => p.statsMod.luckMod *= 1f + (0.15f * mult), rarity));

            if (rarity == StatRarity.Legendary)
            {
                filtered.Add(CreateStatUpgrade("Revival", "+1 Extra Life",
                    (p, mult) => p.statsMod.revivalBonus += Mathf.RoundToInt(mult), rarity));
            }

            // Class-specific stats
            if (classType == Classes.Warrior)
            {
                filtered.Add(CreateStatUpgrade("Increased Area", "+25% Attack Area",
                    (p, mult) => p.statsMod.areaMod *= 1f + (0.25f * mult), rarity));

                filtered.Add(CreateStatUpgrade("Attack Count", "+1 Multi-Attack",
                    (p, mult) => p.statsMod.projectileCountBonus += Mathf.RoundToInt(mult), rarity));

                filtered.Add(CreateStatUpgrade("Knockback", "+25% Knockback",
                    (p, mult) => p.statsMod.knockbackMod *= 1f + (0.25f * mult), rarity));
            }
            else if (classType == Classes.Ranger)
            {
                filtered.Add(CreateStatUpgrade("Projectile Count", "+1 Projectile",
                    (p, mult) => p.statsMod.projectileCountBonus += Mathf.RoundToInt(mult), rarity));

                filtered.Add(CreateStatUpgrade("Pierce", "+1 Pierce",
                    (p, mult) => p.statsMod.pierceCountBonus += Mathf.RoundToInt(mult), rarity));

                filtered.Add(CreateStatUpgrade("Projectile Speed", "+20% Projectile Speed",
                    (p, mult) => p.statsMod.projectileSpeedMod *= 1f + (0.2f * mult), rarity));
            }

            return filtered;
        }

        public void ApplyChoice(Player player, object choice)
        {
            if (choice is StatUpgradeOption statUpgrade)
            {
                statUpgrade.applyUpgrade(player);
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