using System;
using System.Collections.Generic;
using Classes.RangerClass;
using Classes.WarriorClass;
using PlayerDirectory;
using UnityEngine;
using View.UI.Leveling;

namespace Classes.Evolution
{
    [Serializable]
    public class UnlockCondition
    {
        public string subclassName;
        public Func<Player, GameStats, bool> condition;
        public string description;
    }

    public class GameStats
    {
        public int enemiesKilled;
        public int enemiesKilledInWindow;
        public float windowStartTime;
        public float survivalTime;
        public float totalHealingReceived;
        public int maxRangeHits;
        public int weakPointKills;
        public int enemiesSlowedOrImmobilized;
        public float timeSinceLastDeath;
        public int killStreak;
        public float lastKillTime;
    }

    public class SubclassEvolutionManager : MonoBehaviour
    {
        [Header("Evolution Settings")] 
        [SerializeField] private float evolutionCheckInterval = 1f;
        [SerializeField] private GameObject evolutionUIPanel;

        private Player _player;
        private GameStats _gameStats = new GameStats();
        private bool _hasEvolved;
        private float _lastCheckTime;
        private Dictionary<string, UnlockCondition> _unlockConditions;

        private void Awake()
        {
            _player = FindFirstObjectByType<Player>();
            InitializeUnlockConditions();
        }

        private void Update()
        {
            if (_hasEvolved || Time.time - _lastCheckTime < evolutionCheckInterval) return;

            _lastCheckTime = Time.time;
            UpdateGameStats();
            CheckEvolutionConditions();
        }

        private void InitializeUnlockConditions()
        {
            _unlockConditions = new Dictionary<string, UnlockCondition>();

            // Warrior Subclasses
            _unlockConditions["Berserker"] = new UnlockCondition
            {
                subclassName = "Berserker",
                description = "Kill 150 enemies in 90 seconds",
                condition = (player, stats) =>
                {
                    if (stats.enemiesKilledInWindow >= 150)
                    {
                        float timeWindow = Time.time - stats.windowStartTime;
                        return timeWindow <= 90f;
                    }

                    return false;
                }
            };

            _unlockConditions["Paladin"] = new UnlockCondition
            {
                subclassName = "Paladin",
                description = "Survive 4 minutes without dying",
                condition = (player, stats) => stats.timeSinceLastDeath >= 240f
            };

            _unlockConditions["Reaver"] = new UnlockCondition
            {
                subclassName = "Reaver",
                description = "Heal 300 HP total",
                condition = (player, stats) => stats.totalHealingReceived >= 300f
            };

            // Ranger Subclasses
            _unlockConditions["Sniper"] = new UnlockCondition
            {
                subclassName = "Sniper",
                description = "Hit 100 enemies from max range",
                condition = (player, stats) => stats.maxRangeHits >= 100
            };

            _unlockConditions["Hunter"] = new UnlockCondition
            {
                subclassName = "Hunter",
                description = "Get 50 weak point kills",
                condition = (player, stats) => stats.weakPointKills >= 50
            };

            _unlockConditions["Trapper"] = new UnlockCondition
            {
                subclassName = "Trapper",
                description = "Immobilize or slow 80 enemies",
                condition = (player, stats) => stats.enemiesSlowedOrImmobilized >= 80
            };
        }

        private void UpdateGameStats()
        {
            _gameStats.survivalTime = Time.time;
            _gameStats.timeSinceLastDeath += Time.deltaTime;

            // Reset kill window if needed
            if (Time.time - _gameStats.lastKillTime > 5f)
            {
                _gameStats.enemiesKilledInWindow = 0;
                _gameStats.windowStartTime = Time.time;
            }
        }

        private void CheckEvolutionConditions()
        {
            List<string> availableSubclasses = GetAvailableSubclasses();

            if (availableSubclasses.Count > 0)
            {
                ShowEvolutionUI(availableSubclasses);
            }
        }

        private List<string> GetAvailableSubclasses()
        {
            List<string> available = new List<string>();
            PlayerDirectory.Classes playerClass = _player.GetPlayerClassType();

            foreach (KeyValuePair<string, UnlockCondition> kvp in _unlockConditions)
            {
                string subclass = kvp.Key;
                UnlockCondition condition = kvp.Value;

                // Check if subclass matches player class
                bool matchesClass = playerClass switch
                {
                    PlayerDirectory.Classes.Warrior => subclass is "Berserker" or "Paladin" or "Reaver",
                    PlayerDirectory.Classes.Ranger => subclass is "Sniper" or "Hunter" or "Trapper",
                    _ => false
                };

                if (!matchesClass) continue;

                // Check if condition met and permanently unlocked
                if (condition.condition(_player, _gameStats) && !IsSubclassUnlocked(subclass))
                {
                    UnlockSubclass(subclass);
                }

                if (IsSubclassUnlocked(subclass))
                {
                    available.Add(subclass);
                }
            }

            return available;
        }

        private void ShowEvolutionUI(List<string> availableSubclasses)
        {
            _hasEvolved = true;
            Time.timeScale = 0;

            SubclassSelectionUI ui = evolutionUIPanel.GetComponent<SubclassSelectionUI>();
            ui.ShowSubclasses(availableSubclasses, _unlockConditions, this);
            evolutionUIPanel.SetActive(true);
        }

        public void EvolveToSubclass(string subclassName)
        {
            IClass newClass = subclassName switch
            {
                "Berserker" => new Berserker(),
                "Paladin" => new Paladin(),
                "Reaver" => new Reaver(),
                "Sniper" => new Sniper(),
                "Hunter" => new Hunter(),
                "Trapper" => new Trapper(),
                _ => null
            };

            if (newClass != null)
            {
                _player.EvolveClass(newClass, subclassName);
                evolutionUIPanel.SetActive(false);
                Time.timeScale = 1;
                Debug.Log($"Evolved to {subclassName}!");
            }
        }

        // Public methods for tracking stats
        public void OnEnemyKilled()
        {
            _gameStats.enemiesKilled++;
            _gameStats.lastKillTime = Time.time;

            if (_gameStats.enemiesKilledInWindow == 0)
            {
                _gameStats.windowStartTime = Time.time;
            }

            _gameStats.enemiesKilledInWindow++;
        }

        public void OnPlayerHealed(float amount)
        {
            _gameStats.totalHealingReceived += amount;
        }

        public void OnMaxRangeHit()
        {
            _gameStats.maxRangeHits++;
        }

        public void OnWeakPointKill()
        {
            _gameStats.weakPointKills++;
        }

        public void OnEnemySlowedOrImmobilized()
        {
            _gameStats.enemiesSlowedOrImmobilized++;
        }

        public void OnPlayerDeath()
        {
            _gameStats.timeSinceLastDeath = 0;
        }

        // Unlock persistence
        private bool IsSubclassUnlocked(string subclass)
        {
            return PlayerPrefs.GetInt($"Unlocked_{subclass}", 0) == 1;
        }

        private void UnlockSubclass(string subclass)
        {
            PlayerPrefs.SetInt($"Unlocked_{subclass}", 1);
            PlayerPrefs.Save();
            Debug.Log($"Permanently unlocked: {subclass}");
        }

        public GameStats GetGameStats() => _gameStats;
    }
}