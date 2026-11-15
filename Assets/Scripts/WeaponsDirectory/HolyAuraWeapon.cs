using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;

namespace WeaponsDirectory
{
    /// <summary>
    /// A zone around the player that damages every enemy inside on each pulse.
    /// Special = slow strength (0.25 means enemies move at 75% speed while inside).
    /// </summary>
    [CreateAssetMenu(fileName = "HolyAura", menuName = "Weapons/Holy Aura")]
    public class HolyAuraWeapon : WeaponDefinition
    {
        [Header("Holy Aura")]
        [SerializeField] private float baseRadius = 1.6f;
        [SerializeField, Range(0f, 1f)] private float idleAlpha = 0.18f;
        [SerializeField, Range(0f, 1f)] private float pulseAlpha = 0.45f;

        public HolyAuraWeapon()
        {
            SetDefaults("Holy Aura", "A holy field burns enemies around you.",
                new Color(1f, 0.9f, 0.45f),
                new WeaponLevelStats("A holy field burns enemies around you.", 4f, 0.8f, 1f, 1),
                new WeaponLevelStats("Larger field.", 4f, 0.8f, 1.2f, 1),
                new WeaponLevelStats("+50% damage.", 6f, 0.8f, 1.2f, 1),
                new WeaponLevelStats("Enemies inside are slowed.", 6f, 0.8f, 1.2f, 1, special: 0.25f),
                new WeaponLevelStats("Larger field, pulses faster.", 6f, 0.7f, 1.4f, 1, special: 0.25f),
                new WeaponLevelStats("+50% damage.", 9f, 0.7f, 1.4f, 1, special: 0.25f),
                new WeaponLevelStats("Huge field, stronger slow.", 10f, 0.6f, 1.7f, 1, special: 0.4f));
        }

        public override string SpecialStatName => "Slow";
        protected override Sprite DefaultIcon => WeaponVisuals.Aura;

        public override WeaponRuntime CreateRuntime(Player player, Transform root) =>
            new Runtime(this, player, root);

        private class Runtime : WeaponRuntime
        {
            private readonly HolyAuraWeapon _definition;
            private readonly SpriteRenderer _renderer;
            private float _timer;
            private float _flash;

            public Runtime(HolyAuraWeapon definition, Player player, Transform root) : base(definition, player, root)
            {
                _definition = definition;
                // Offset 1: above the ground, below the other weapons
                _renderer = WeaponVisuals.CreateRenderer("Aura", root, WeaponVisuals.Aura, definition.Color, 1);
                _timer = 0.2f;
            }

            public override void Tick(float deltaTime)
            {
                float radius = _definition.baseRadius * Area;
                Transform aura = _renderer.transform;
                aura.position = PlayerTransform.position;
                aura.localScale = new Vector3(radius * 2f, radius * 2f, 1f);

                _flash = Mathf.Max(0f, _flash - deltaTime * 4f);
                Color color = _definition.Color;
                color.a = Mathf.Lerp(_definition.idleAlpha, _definition.pulseAlpha, _flash);
                _renderer.color = color;

                _timer -= deltaTime;
                if (_timer > 0f) return;

                float cooldown = Cooldown;
                _timer += cooldown;
                _flash = 1f;

                Vector2 center = PlayerPosition;
                float slow = Mathf.Clamp01(Special);

                foreach (Enemy enemy in EnemiesInRadius(center, radius))
                {
                    Hit(enemy);
                    if (slow > 0f && !enemy.IsDead() && enemy.gameObject.activeInHierarchy)
                    {
                        // Lasts until the next pulse, so the slow drops off soon after leaving the field
                        enemy.Slow(1f - slow, cooldown + 0.1f);
                    }
                }
            }
        }
    }
}
