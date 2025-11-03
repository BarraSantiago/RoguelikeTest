using System;
using System.Collections.Generic;
using PowerUps;
using UnityEngine;

namespace PlayerDirectory.LevelingSystem
{
    [Serializable]
    public enum UpgradeType
    {
        StatUpgrade,
        PowerUpUpgrade
    }

    [Serializable]
    public class StatUpgradeOption
    {
        public string name;
        public string description;
        public Action<Player> applyUpgrade;
    }

    public class LevelUpManager : MonoBehaviour
    {
        [SerializeField] private int numberOfChoices = 3;

        private List<StatUpgradeOption> _availableStatUpgrades;
        private HashSet<object> _banishedChoices = new HashSet<object>();

        private void Awake()
        {
            InitializeStatUpgrades();
        }

        private void InitializeStatUpgrades()
        {
            _availableStatUpgrades = new List<StatUpgradeOption>
            {
                new()
                {
                    name = "Increased Damage",
                    description = "+20% Damage",
                    applyUpgrade = player => player.statsMod.damageMod *= 1.2f
                },
                new()
                {
                    name = "Attack Speed",
                    description = "+15% Attack Speed",
                    applyUpgrade = player => player.statsMod.attackSpeedMod *= 0.85f
                },
                new()
                {
                    name = "Increased Range",
                    description = "+25% Range",
                    applyUpgrade = player => player.statsMod.rangeMod *= 1.25f
                },
                new()
                {
                    name = "Critical Chance",
                    description = "+5% Critical Chance",
                    applyUpgrade = player => player.playerStats.criticalChance += 0.05f
                },
                new()
                {
                    name = "Movement Speed",
                    description = "+15% Speed",
                    applyUpgrade = player => player.playerStats.speed *= 1.15f
                },
                new()
                {
                    name = "Max Health",
                    description = "+20% Health",
                    applyUpgrade = player =>
                    {
                        player.playerStats.health *= 1.2f;
                        player.playerStats.maxHealth = player.playerStats.health;
                    }
                },
                new()
                {
                    name = "Health Regeneration",
                    description = "+1 HP/s Regen",
                    applyUpgrade = player => player.statsMod.healthRegenBonus += 1f
                },
                new()
                {
                    name = "Armor",
                    description = "+5 Armor",
                    applyUpgrade = player => player.statsMod.armorBonus += 5f
                },
                new()
                {
                    name = "Cooldown Reduction",
                    description = "+10% CDR",
                    applyUpgrade = player => player.statsMod.cooldownReductionBonus += 0.1f
                },
                new()
                {
                    name = "Pickup Radius",
                    description = "+25% Pickup Range",
                    applyUpgrade = player => player.statsMod.pickupRadiusMod *= 1.25f
                },
                new()
                {
                    name = "Magnetism",
                    description = "+30% Attraction Speed",
                    applyUpgrade = player => player.statsMod.magnetismMod *= 1.3f
                },
                new()
                {
                    name = "Experience Gain",
                    description = "+20% XP",
                    applyUpgrade = player => player.statsMod.growthMod *= 1.2f
                },
                new()
                {
                    name = "Luck",
                    description = "+15% Luck",
                    applyUpgrade = player => player.statsMod.luckMod *= 1.15f
                },
                new()
                {
                    name = "Projectile Count",
                    description = "+1 Projectile",
                    applyUpgrade = player => player.statsMod.projectileCountBonus++
                },
                new()
                {
                    name = "Pierce",
                    description = "+1 Pierce",
                    applyUpgrade = player => player.statsMod.pierceCountBonus++
                },
                new()
                {
                    name = "Revival",
                    description = "+1 Extra Life",
                    applyUpgrade = player => player.statsMod.revivalBonus++
                }
            };
        }

        public List<object> GenerateLevelUpChoices(Player player)
        {
            List<object> choices = new List<object>();
            List<StatUpgradeOption> availableStats = new List<StatUpgradeOption>(_availableStatUpgrades);
            List<PowerUp> upgradablePowerUps = player.powerUps.FindAll(p => p.CanUpgrade());

            // Remove banished choices
            availableStats.RemoveAll(stat => _banishedChoices.Contains(stat));
            upgradablePowerUps.RemoveAll(powerUp => _banishedChoices.Contains(powerUp));

            // Ensure we have enough options
            int totalAvailable = availableStats.Count + upgradablePowerUps.Count;
            int actualChoices = Mathf.Min(numberOfChoices, totalAvailable);

            for (int i = 0; i < actualChoices; i++)
            {
                // Mix stat upgrades and power-up upgrades
                bool choosePowerUp = upgradablePowerUps.Count > 0 &&
                                     (availableStats.Count == 0 || UnityEngine.Random.value > 0.5f);

                if (choosePowerUp)
                {
                    int randomIndex = UnityEngine.Random.Range(0, upgradablePowerUps.Count);
                    choices.Add(upgradablePowerUps[randomIndex]);
                    upgradablePowerUps.RemoveAt(randomIndex);
                }
                else if (availableStats.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, availableStats.Count);
                    choices.Add(availableStats[randomIndex]);
                    availableStats.RemoveAt(randomIndex);
                }
            }

            return choices;
        }

        public void ApplyChoice(Player player, object choice)
        {
            if (choice is StatUpgradeOption statUpgrade)
            {
                statUpgrade.applyUpgrade(player);
                Debug.Log($"Applied stat upgrade: {statUpgrade.name}");
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

        // For meta-progression: some choices might unlock later
        public void UnlockStatUpgrade(StatUpgradeOption upgrade)
        {
            if (!_availableStatUpgrades.Contains(upgrade))
            {
                _availableStatUpgrades.Add(upgrade);
            }
        }
    }
}