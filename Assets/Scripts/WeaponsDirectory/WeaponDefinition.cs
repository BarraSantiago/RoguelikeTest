using System;
using System.Collections.Generic;
using System.Text;
using PlayerDirectory;
using UnityEngine;

namespace WeaponsDirectory
{
    /// <summary>
    /// Stats for one weapon level. Each entry is the full set of values at that level (not a delta),
    /// so designers can read and tune a level in isolation.
    /// </summary>
    [Serializable]
    public class WeaponLevelStats
    {
        [Tooltip("Shown on the level-up card when upgrading to this level.")]
        public string description;
        public float damage = 10f;
        [Tooltip("Seconds between attacks (for Orbiting Blades: seconds before the same enemy can be hit again).")]
        public float cooldown = 1f;
        [Tooltip("Size multiplier (radius, reach, projectile size).")]
        public float area = 1f;
        [Tooltip("Number of blades / bolts / projectiles.")]
        public int amount = 1;
        [Tooltip("Projectile or rotation speed multiplier.")]
        public float speed = 1f;
        [Tooltip("Lifetime / travel time multiplier.")]
        public float duration = 1f;
        [Tooltip("Weapon-specific value. See the weapon's SpecialStatName.")]
        public float special;

        public WeaponLevelStats() { }

        public WeaponLevelStats(string description, float damage, float cooldown, float area, int amount,
            float speed = 1f, float duration = 1f, float special = 0f)
        {
            this.description = description;
            this.damage = damage;
            this.cooldown = cooldown;
            this.area = area;
            this.amount = amount;
            this.speed = speed;
            this.duration = duration;
            this.special = special;
        }
    }

    /// <summary>
    /// Data for a weapon the player can acquire from level-ups. Assets live in Resources/Weapons
    /// and are loaded automatically by LevelUpManager. The asset holds data only: all per-run
    /// state lives in the <see cref="WeaponRuntime"/> it creates, so nothing leaks between runs.
    /// </summary>
    public abstract class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [Tooltip("Optional. A generated shape is used when empty.")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Color color = Color.white;
        [Tooltip("One entry per level. Level 1 is the first entry.")]
        [SerializeField] protected List<WeaponLevelStats> levels = new();

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string Description => description;
        public Color Color => color;
        public int MaxLevel => Mathf.Max(1, levels.Count);
        public Sprite Icon => icon ? icon : DefaultIcon;

        /// <summary>Label for <see cref="WeaponLevelStats.special"/>, or null if the weapon doesn't use it.</summary>
        public virtual string SpecialStatName => null;

        protected virtual Sprite DefaultIcon => WeaponVisuals.Circle;

        /// <summary>
        /// Called from subclass constructors so a freshly created asset (or one whose fields are
        /// missing in YAML) starts with a playable configuration. Values saved in the asset win.
        /// </summary>
        protected void SetDefaults(string defaultName, string defaultDescription, Color defaultColor,
            params WeaponLevelStats[] defaultLevels)
        {
            displayName = defaultName;
            description = defaultDescription;
            color = defaultColor;
            levels = new List<WeaponLevelStats>(defaultLevels);
        }

        public WeaponLevelStats GetLevel(int level)
        {
            if (levels.Count == 0) return new WeaponLevelStats();
            return levels[Mathf.Clamp(level - 1, 0, levels.Count - 1)];
        }

        public abstract WeaponRuntime CreateRuntime(Player player, Transform root);

        /// <summary>Human-readable list of what changes between two levels, e.g. "Damage 8 → 12".</summary>
        public string DescribeUpgrade(int fromLevel, int toLevel)
        {
            WeaponLevelStats from = GetLevel(fromLevel);
            WeaponLevelStats to = GetLevel(toLevel);
            StringBuilder sb = new StringBuilder();

            AppendChange(sb, "Damage", from.damage, to.damage, "F0");
            AppendChange(sb, "Cooldown", from.cooldown, to.cooldown, "0.##", "s");
            AppendChange(sb, "Area", from.area, to.area, "P0");
            AppendChange(sb, "Amount", from.amount, to.amount, "F0");
            AppendChange(sb, "Speed", from.speed, to.speed, "P0");
            AppendChange(sb, "Duration", from.duration, to.duration, "P0");
            if (SpecialStatName != null)
            {
                AppendChange(sb, SpecialStatName, from.special, to.special, "0.##");
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>Key stats at a level, for a weapon that isn't owned yet.</summary>
        public string DescribeLevel(int level)
        {
            WeaponLevelStats stats = GetLevel(level);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Damage {stats.damage:F0}");
            sb.AppendLine($"Cooldown {stats.cooldown:0.##}s");
            if (stats.amount > 1) sb.AppendLine($"Amount {stats.amount}");
            return sb.ToString().TrimEnd();
        }

        private static void AppendChange(StringBuilder sb, string label, float from, float to, string format,
            string suffix = "")
        {
            if (Mathf.Approximately(from, to)) return;
            sb.AppendLine($"{label} {from.ToString(format)}{suffix} → {to.ToString(format)}{suffix}");
        }
    }

    /// <summary>A level-up option that either grants a new weapon or upgrades an owned one.</summary>
    public class WeaponChoice
    {
        public WeaponDefinition Definition { get; }
        /// <summary>0 when the weapon is not owned yet.</summary>
        public int CurrentLevel { get; }
        public int NextLevel => CurrentLevel + 1;
        public bool IsNew => CurrentLevel == 0;

        public WeaponChoice(WeaponDefinition definition, int currentLevel)
        {
            Definition = definition;
            CurrentLevel = currentLevel;
        }
    }
}
