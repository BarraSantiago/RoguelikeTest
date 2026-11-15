using System.Collections.Generic;
using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;

namespace WeaponsDirectory
{
    /// <summary>
    /// Strikes the nearest enemy, then jumps to nearby enemies. Amount = number of bolts per cast.
    /// Special = number of jumps after the first hit (plus the player's chain count bonus).
    /// </summary>
    [CreateAssetMenu(fileName = "ChainLightning", menuName = "Weapons/Chain Lightning")]
    public class ChainLightningWeapon : WeaponDefinition
    {
        [Header("Chain Lightning")]
        [SerializeField] private float targetRange = 6f;
        [SerializeField] private float jumpRange = 2.5f;
        [Tooltip("Damage multiplier applied on each jump.")]
        [SerializeField, Range(0.1f, 1f)] private float jumpFalloff = 0.85f;
        [SerializeField] private float boltWidth = 0.08f;
        [SerializeField] private float boltFadeTime = 0.15f;

        public ChainLightningWeapon()
        {
            SetDefaults("Chain Lightning", "Lightning strikes the nearest enemy and jumps between foes.",
                new Color(0.6f, 0.85f, 1f),
                new WeaponLevelStats("Lightning jumps between enemies.", 14f, 1.6f, 1f, 1, special: 2f),
                new WeaponLevelStats("+1 jump.", 14f, 1.6f, 1f, 1, special: 3f),
                new WeaponLevelStats("+40% damage.", 20f, 1.6f, 1f, 1, special: 3f),
                new WeaponLevelStats("+1 bolt per cast.", 20f, 1.6f, 1f, 2, special: 3f),
                new WeaponLevelStats("Casts faster, +1 jump.", 20f, 1.3f, 1.1f, 2, special: 4f),
                new WeaponLevelStats("+40% damage.", 28f, 1.3f, 1.1f, 2, special: 4f),
                new WeaponLevelStats("+1 bolt, +1 jump.", 30f, 1.2f, 1.2f, 3, special: 5f));
        }

        public override string SpecialStatName => "Jumps";
        protected override Sprite DefaultIcon => WeaponVisuals.Circle;

        public override WeaponRuntime CreateRuntime(Player player, Transform root) =>
            new Runtime(this, player, root);

        private class Runtime : WeaponRuntime
        {
            private const int SegmentsPerJump = 3;

            private readonly ChainLightningWeapon _definition;
            private readonly List<SpriteRenderer> _segments = new();
            private readonly List<float> _segmentAge = new();
            private readonly HashSet<Enemy> _struck = new();
            private readonly List<Vector2> _points = new();
            private float _timer = 0.5f;

            public Runtime(ChainLightningWeapon definition, Player player, Transform root) : base(definition, player, root)
            {
                _definition = definition;
            }

            public override void Tick(float deltaTime)
            {
                FadeSegments(deltaTime);

                _timer -= deltaTime;
                if (_timer > 0f) return;

                Vector2 origin = PlayerPosition;
                float range = _definition.targetRange * Area;
                if (!FindNearestEnemy(origin, range))
                {
                    // Nothing in range: check again soon instead of wasting the cooldown
                    _timer = 0.1f;
                    return;
                }

                _timer = Cooldown;
                _struck.Clear();

                int bolts = Amount;
                for (int i = 0; i < bolts; i++)
                {
                    CastBolt(origin, range);
                }
            }

            private void CastBolt(Vector2 origin, float range)
            {
                int jumps = Mathf.Max(0, Mathf.RoundToInt(Special) + Mods.chainCountBonus);
                float jumpRange = _definition.jumpRange * Area;
                float damageMultiplier = 1f;

                Vector2 from = origin;
                Enemy target = FindNearestEnemy(origin, range, _struck);

                for (int i = 0; i <= jumps && target; i++)
                {
                    // Read the position before hitting: a kill returns the enemy to its pool
                    Vector2 targetPosition = target.Position;
                    _struck.Add(target);
                    DrawBolt(from, targetPosition);
                    Hit(target, damageMultiplier);

                    from = targetPosition;
                    damageMultiplier *= _definition.jumpFalloff;
                    target = FindNearestEnemy(from, jumpRange, _struck);
                }
            }

            private void DrawBolt(Vector2 from, Vector2 to)
            {
                // A few jagged sub-segments read as lightning better than a straight line
                _points.Clear();
                _points.Add(from);
                Vector2 delta = to - from;
                Vector2 normal = new Vector2(-delta.y, delta.x).normalized;
                for (int i = 1; i < SegmentsPerJump; i++)
                {
                    float t = (float)i / SegmentsPerJump;
                    float offset = Random.Range(-0.25f, 0.25f) * Mathf.Min(1f, delta.magnitude * 0.3f);
                    _points.Add(from + delta * t + normal * offset);
                }
                _points.Add(to);

                for (int i = 0; i < _points.Count - 1; i++)
                {
                    SpriteRenderer segment = GetSegment();
                    WeaponVisuals.StretchBetween(segment.transform, _points[i], _points[i + 1], _definition.boltWidth);
                }
            }

            private SpriteRenderer GetSegment()
            {
                for (int i = 0; i < _segments.Count; i++)
                {
                    if (_segments[i].gameObject.activeSelf) continue;
                    _segments[i].gameObject.SetActive(true);
                    _segmentAge[i] = 0f;
                    return _segments[i];
                }

                SpriteRenderer created = WeaponVisuals.CreateRenderer("Bolt", Root, WeaponVisuals.Square,
                    _definition.Color, 3);
                _segments.Add(created);
                _segmentAge.Add(0f);
                return created;
            }

            private void FadeSegments(float deltaTime)
            {
                for (int i = 0; i < _segments.Count; i++)
                {
                    SpriteRenderer segment = _segments[i];
                    if (!segment.gameObject.activeSelf) continue;

                    _segmentAge[i] += deltaTime;
                    float remaining = 1f - _segmentAge[i] / _definition.boltFadeTime;
                    if (remaining <= 0f)
                    {
                        segment.gameObject.SetActive(false);
                        continue;
                    }

                    Color color = _definition.Color;
                    color.a = remaining;
                    segment.color = color;
                }
            }
        }
    }
}
