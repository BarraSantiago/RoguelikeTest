using System;
using UnityEngine;

namespace Achievements
{
    [Serializable]
    public class ShopUnlock
    {
        public string id;
        public ShopUnlockType type;
        public string targetId;
        public int baseCost;
        public int currentCost;
        public bool isPurchased;
        public int maxPurchases;
        public int currentPurchases;
        public float priceMultiplier;

        public ShopUnlock(string id, ShopUnlockType type, string targetId, int baseCost, int maxPurchases = 1, float priceMultiplier = 1.5f)
        {
            this.id = id;
            this.type = type;
            this.targetId = targetId;
            this.baseCost = baseCost;
            this.currentCost = baseCost;
            this.maxPurchases = maxPurchases;
            this.currentPurchases = 0;
            this.isPurchased = false;
            this.priceMultiplier = priceMultiplier;
        }

        public void IncrementPurchase()
        {
            currentPurchases++;
            if (currentPurchases >= maxPurchases)
            {
                isPurchased = true;
            }
            else
            {
                currentCost = Mathf.RoundToInt(baseCost * Mathf.Pow(priceMultiplier, currentPurchases));
            }
        }

        public bool CanPurchaseMore() => currentPurchases < maxPurchases;
    }
    
    public enum ShopUnlockType
    {
        Subclass,
        StatBonus,
        Skin
    }

    [Serializable]
    public class StatBonus
    {
        public string id;
        public string name;
        public string description;
        public StatBonusType type;
        public float value;

        public StatBonus(string id, string name, string description, StatBonusType type, float value)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.type = type;
            this.value = value;
        }
    }

    public enum StatBonusType
    {
        Growth,
        Reroll,
        Skip,
        Banish,
        MaxHealth,
        Armor,
        HealthRegen,
        PickupRadius,
        Luck,
        CooldownReduction
    }
}