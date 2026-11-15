using System.Collections.Generic;
using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;

namespace WeaponsDirectory
{
    /// <summary>
    /// Blades circle the player and cut anything they touch. Cooldown is the time before the
    /// same enemy can be hit again, so blades don't shred an enemy every frame.
    /// </summary>
    [CreateAssetMenu(fileName = "OrbitingBlades", menuName = "Weapons/Orbiting Blades")]
    public class OrbitingBladesWeapon : WeaponDefinition
    {
        [Header("Orbiting Blades")]
        [SerializeField] private float orbitRadius = 1.4f;
        [SerializeField] private float bladeHitRadius = 0.3f;
        [Tooltip("Degrees per second at speed 1.")]
        [SerializeField] private float rotationSpeed = 200f;
        [SerializeField] private float knockback = 2f;

        public OrbitingBladesWeapon()
        {
            SetDefaults("Orbiting Blades", "Blades circle around you, cutting enemies they touch.",
                new Color(0.75f, 0.9f, 1f),
                new WeaponLevelStats("Blades circle around you.", 8f, 0.6f, 1f, 2),
                new WeaponLevelStats("+1 blade.", 8f, 0.6f, 1f, 3),
                new WeaponLevelStats("+50% damage.", 12f, 0.6f, 1f, 3),
                new WeaponLevelStats("Wider and faster orbit.", 12f, 0.6f, 1.25f, 3, 1.25f),
                new WeaponLevelStats("+1 blade.", 12f, 0.6f, 1.25f, 4, 1.25f),
                new WeaponLevelStats("More damage, hits more often.", 16f, 0.45f, 1.25f, 4, 1.25f),
                new WeaponLevelStats("+1 blade, even wider orbit.", 18f, 0.45f, 1.5f, 5, 1.4f));
        }

        protected override Sprite DefaultIcon => WeaponVisuals.Blade;

        public override WeaponRuntime CreateRuntime(Player player, Transform root) =>
            new Runtime(this, player, root);

        private class Runtime : WeaponRuntime
        {
            private readonly OrbitingBladesWeapon _definition;
            private readonly List<Transform> _blades = new();
            private readonly Dictionary<Enemy, float> _nextHitTime = new();
            private readonly List<Enemy> _hits = new();
            private readonly List<Enemy> _nearBlade = new();
            private float _angle;
            private float _time;

            public Runtime(OrbitingBladesWeapon definition, Player player, Transform root)
                : base(definition, player, root)
            {
                _definition = definition;
            }

            public override void Tick(float deltaTime)
            {
                _time += deltaTime;
                SyncBladeCount();

                float area = Area;
                float radius = _definition.orbitRadius * area;
                float hitRadius = _definition.bladeHitRadius * area;
                _angle = (_angle + _definition.rotationSpeed * Speed * deltaTime) % 360f;

                Vector2 center = PlayerPosition;
                Vector3 bladeScale = new Vector3(0.9f, 0.9f, 1f) * Mathf.Sqrt(area);
                _hits.Clear();

                for (int i = 0; i < _blades.Count; i++)
                {
                    float angle = _angle + 360f * i / _blades.Count;
                    float rad = angle * Mathf.Deg2Rad;
                    Vector2 position = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

                    Transform blade = _blades[i];
                    blade.position = position;
                    // Point along the direction of travel
                    blade.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
                    blade.localScale = bladeScale;

                    CollectEnemies(position, hitRadius, _nearBlade);
                    foreach (Enemy enemy in _nearBlade)
                    {
                        if (_hits.Contains(enemy)) continue;
                        if (_nextHitTime.TryGetValue(enemy, out float next) && _time < next) continue;
                        _hits.Add(enemy);
                    }
                }

                // Damage after collecting: kills modify EnemyManager.Enemies
                float hitInterval = Cooldown;
                foreach (Enemy enemy in _hits)
                {
                    _nextHitTime[enemy] = _time + hitInterval;
                    Hit(enemy, 1f, _definition.knockback, center);
                }

                if (_nextHitTime.Count > 128) PruneHitTimes();
            }

            private void SyncBladeCount()
            {
                int wanted = Amount;
                while (_blades.Count < wanted)
                {
                    SpriteRenderer blade = WeaponVisuals.CreateRenderer($"Blade {_blades.Count + 1}", Root,
                        _definition.Icon, _definition.Color, 2);
                    _blades.Add(blade.transform);
                }

                while (_blades.Count > wanted)
                {
                    Transform last = _blades[_blades.Count - 1];
                    _blades.RemoveAt(_blades.Count - 1);
                    Object.Destroy(last.gameObject);
                }
            }

            private void PruneHitTimes()
            {
                List<Enemy> expired = new List<Enemy>();
                foreach (KeyValuePair<Enemy, float> entry in _nextHitTime)
                {
                    if (!entry.Key || entry.Value <= _time) expired.Add(entry.Key);
                }

                foreach (Enemy enemy in expired) _nextHitTime.Remove(enemy);
            }
        }
    }
}
