using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Achievements
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        private Dictionary<string, ShopUnlock> _availableUnlocks;
        private Dictionary<string, StatBonus> _statBonuses;

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            _availableUnlocks = new Dictionary<string, ShopUnlock>();
            _statBonuses = new Dictionary<string, StatBonus>();

            InitializeStatBonuses();
            LoadShopData();
            
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.onShopItemUnlocked.AddListener(RefreshShopData);
            }
        }
        
        public void RefreshShopData()
        {
            LoadShopData();
        }

        private void InitializeStatBonuses()
        {
            _statBonuses["reroll"] = new StatBonus("reroll", "+1 Reroll", "Additional reroll charge for level-ups",
                StatBonusType.Reroll, 1);
            _statBonuses["skip"] = new StatBonus("skip", "+1 Skip", "Additional skip charge for level-ups",
                StatBonusType.Skip, 1);
            _statBonuses["banish"] = new StatBonus("banish", "+1 Banish", "Additional banish charge for level-ups",
                StatBonusType.Banish, 1);
            _statBonuses["growth"] = new StatBonus("growth", "+10% XP Gain", "Increase experience gained",
                StatBonusType.Growth, 0.1f);
            _statBonuses["health_regen"] = new StatBonus("health_regen", "+1 HP/s Regen", "Regenerate health over time",
                StatBonusType.HealthRegen, 1f);
            _statBonuses["pickup_radius"] = new StatBonus("pickup_radius", "+15% Pickup Radius",
                "Increase item collection range", StatBonusType.PickupRadius, 0.15f);
            _statBonuses["luck"] = new StatBonus("luck", "+5 Luck", "Improve drop rates and proc chances",
                StatBonusType.Luck, 5f);
            _statBonuses["max_health"] = new StatBonus("max_health", "+20 Max Health", "Increase maximum health",
                StatBonusType.MaxHealth, 20f);
            _statBonuses["armor"] =
                new StatBonus("armor", "+2 Armor", "Reduce incoming damage", StatBonusType.Armor, 2f);
            _statBonuses["cooldown"] = new StatBonus("cooldown", "+5% CDR", "Reduce ability cooldowns",
                StatBonusType.CooldownReduction, 0.05f);
        }

        private void LoadShopData()
        {
            Dictionary<string, Achievement> achievements = AchievementManager.Instance.GetAllAchievements();

            foreach (Achievement achievement in achievements.Values)
            {
                foreach (AchievementMilestone milestone in achievement.milestones)
                {
                    if (milestone.shopUnlock != null && milestone.isCompleted)
                    {
                        string key = $"ShopUnlock_{milestone.shopUnlock.id}";
                        if (PlayerPrefs.GetInt(key, 0) == 1)
                        {
                            ShopUnlock unlock = milestone.shopUnlock;

                            // Load purchase count and recalculate price
                            unlock.currentPurchases = PlayerPrefs.GetInt($"ShopPurchaseCount_{unlock.id}", 0);
                            unlock.isPurchased = unlock.currentPurchases >= unlock.maxPurchases;

                            if (!unlock.isPurchased)
                            {
                                unlock.currentCost = Mathf.RoundToInt(unlock.baseCost *
                                                                      Mathf.Pow(unlock.priceMultiplier,
                                                                          unlock.currentPurchases));
                            }

                            _availableUnlocks[unlock.id] = unlock;
                        }
                    }
                }
            }
        }

        public bool PurchaseItem(string unlockId)
        {
            if (!_availableUnlocks.TryGetValue(unlockId, out ShopUnlock unlock))
            {
                Debug.LogWarning($"Shop item {unlockId} not available");
                return false;
            }

            if (!unlock.CanPurchaseMore())
            {
                Debug.LogWarning($"Shop item {unlockId} fully purchased");
                return false;
            }

            if (!AchievementManager.Instance.SpendCoins(unlock.currentCost))
            {
                Debug.LogWarning($"Not enough coins to purchase {unlockId}");
                return false;
            }

            ApplyPurchase(unlock);
            unlock.IncrementPurchase();

            PlayerPrefs.SetInt($"ShopPurchaseCount_{unlockId}", unlock.currentPurchases);
            PlayerPrefs.SetInt($"ShopPurchased_{unlockId}", unlock.isPurchased ? 1 : 0);
            PlayerPrefs.Save();

            Debug.Log($"Purchased: {unlockId} (Level {unlock.currentPurchases}/{unlock.maxPurchases})");
            return true;
        }

        public void DebugUnlockAllShopItems()
        {
            Dictionary<string, Achievement> achievements = AchievementManager.Instance.GetAllAchievements();
        
            foreach (Achievement achievement in achievements.Values)
            {
                foreach (AchievementMilestone milestone in achievement.milestones)
                {
                    if (milestone.shopUnlock != null)
                    {
                        string key = $"ShopUnlock_{milestone.shopUnlock.id}";
                        PlayerPrefs.SetInt(key, 1);
                    }
                }
            }
        
            PlayerPrefs.Save();
            RefreshShopData();
            Debug.Log("[DEBUG] All shop items unlocked");
        }
        
        public void DebugClearAllPurchases()
        {
            // Clear all shop purchases
            foreach (ShopUnlock unlock in _availableUnlocks.Values)
            {
                PlayerPrefs.DeleteKey($"ShopPurchaseCount_{unlock.id}");
                PlayerPrefs.DeleteKey($"ShopPurchased_{unlock.id}");
                PlayerPrefs.DeleteKey($"SubclassUnlocked_{unlock.targetId}");
                PlayerPrefs.DeleteKey($"StatBonus_{unlock.targetId}");
                PlayerPrefs.DeleteKey($"SkinUnlocked_{unlock.targetId}");
            }
        
            // Clear all stat bonuses
            foreach (string bonusId in _statBonuses.Keys)
            {
                PlayerPrefs.DeleteKey($"StatBonus_{bonusId}");
            }
        
            // Clear subclass unlocks
            string[] subclasses = { "Berserker", "Paladin", "Reaver", "Sniper", "Hunter", "Trapper" };
            foreach (string subclass in subclasses)
            {
                PlayerPrefs.DeleteKey($"SubclassUnlocked_{subclass}");
            }
        
            PlayerPrefs.Save();
            RefreshShopData();
            Debug.Log("[DEBUG] All purchases cleared");
        }
        
        private void ApplyPurchase(ShopUnlock unlock)
        {
            switch (unlock.type)
            {
                case ShopUnlockType.Subclass:
                    PlayerPrefs.SetInt($"SubclassUnlocked_{unlock.targetId}", 1);
                    break;

                case ShopUnlockType.StatBonus:
                    string key = $"StatBonus_{unlock.targetId}";
                    int current = PlayerPrefs.GetInt(key, 0);
                    PlayerPrefs.SetInt(key, current + 1);
                    break;

                case ShopUnlockType.Skin:
                    PlayerPrefs.SetInt($"SkinUnlocked_{unlock.targetId}", 1);
                    break;
            }

            PlayerPrefs.Save();
        }

        public List<ShopUnlock> GetAvailableUnlocks()
        {
            return _availableUnlocks.Values.ToList();
        }

        public StatBonus GetStatBonus(string bonusId)
        {
            return _statBonuses.TryGetValue(bonusId, out StatBonus bonus) ? bonus : null;
        }

        public Dictionary<StatBonusType, float> GetPurchasedStatBonuses()
        {
            Dictionary<StatBonusType, float> bonuses = new Dictionary<StatBonusType, float>();

            foreach (StatBonus stat in _statBonuses.Values)
            {
                int purchases = PlayerPrefs.GetInt($"StatBonus_{stat.id}", 0);
                if (purchases > 0)
                {
                    if (!bonuses.ContainsKey(stat.type))
                        bonuses[stat.type] = 0;
                    bonuses[stat.type] += stat.value * purchases;
                }
            }

            return bonuses;
        }

        public bool IsSubclassUnlocked(string subclassName)
        {
            return PlayerPrefs.GetInt($"SubclassUnlocked_{subclassName}", 0) == 1;
        }
    }
}