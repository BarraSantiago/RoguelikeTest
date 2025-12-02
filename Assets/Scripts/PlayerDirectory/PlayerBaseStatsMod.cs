using System;

namespace PlayerDirectory
{
    /// <summary>
    /// Represents a set of modifiers and bonuses applied to a player character's base stats.
    /// </summary>
    /// <remarks>
    /// Fields ending with <c>Mod</c> are multipliers (e.g., <c>speedMod</c> multiplies the base speed),
    /// while fields ending with <c>Bonus</c> are additive (e.g., <c>healthRegenBonus</c> adds to the base health regeneration).
    /// When calculating a player's final stats, the base value is first multiplied by the relevant <c>Mod</c> field,
    /// then increased by the corresponding <c>Bonus</c> field (if applicable).
    /// This class is typically used to aggregate all stat modifications affecting a player, such as from items, buffs, or abilities.
    /// </remarks>
    [Serializable]
    public class PlayerBaseStatsMod : BaseStatsMod
    {
        private const float DEFAULT_MULTIPLIER = 1f;
        private const float DEFAULT_BONUS = 0f;
        private const int DEFAULT_INT_BONUS = 0;

        public float speedMod = DEFAULT_MULTIPLIER;
        public float sAttackSpeedMod = DEFAULT_MULTIPLIER;
        public float defenseSpeedMod = DEFAULT_MULTIPLIER;
        public float defenseMod = DEFAULT_MULTIPLIER;
        public float knockbackMod = DEFAULT_MULTIPLIER;
        public float slowResistanceMod = DEFAULT_MULTIPLIER;

        public float healthRegenBonus = DEFAULT_BONUS;
        public float armorBonus = DEFAULT_BONUS;
        public float pickupRadiusMod = DEFAULT_MULTIPLIER;
        public float magnetismMod = DEFAULT_MULTIPLIER;
        public float luckMod = DEFAULT_MULTIPLIER;
        public float greedMod = DEFAULT_MULTIPLIER;
        public float growthMod = DEFAULT_MULTIPLIER;
        public float curseBonus = DEFAULT_BONUS;
        public int revivalBonus = DEFAULT_INT_BONUS;
        public int rerollBonus = DEFAULT_INT_BONUS;
        public int skipBonus = DEFAULT_INT_BONUS;
        public int banishBonus = DEFAULT_INT_BONUS;
    }
}