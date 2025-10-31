using System;
using System.Collections.Generic;
using UnityEngine;
using PowerUps;

namespace PlayerDirectory
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
                    applyUpgrade = player => player.playerStats.health *= 1.2f
                }
            };
        }

        public List<object> GenerateLevelUpChoices(Player player)
        {
            List<object> choices = new List<object>();
            List<StatUpgradeOption> availableStats = new List<StatUpgradeOption>(_availableStatUpgrades);
            List<PowerUp> upgradablePowerUps = player.powerUps.FindAll(p => p.CanUpgrade());

            for (int i = 0; i < numberOfChoices; i++)
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
            }
            else if (choice is PowerUp powerUp)
            {
                powerUp.Upgrade();
            }
        }
    }
}