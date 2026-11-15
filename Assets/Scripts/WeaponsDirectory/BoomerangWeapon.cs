using System.Collections.Generic;
using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;

namespace WeaponsDirectory
{
    /// <summary>
    /// Thrown toward the nearest enemy, then flies back to the player. Pierces everything and can
    /// hit each enemy once on the way out and once on the way back.
    /// </summary>
    [CreateAssetMenu(fileName = "Boomerang", menuName = "Weapons/Boomerang")]
    public class BoomerangWeapon : WeaponDefinition
    {
        [Header("Boomerang")]
        [SerializeField] private float throwSpeed = 7f;
        [Tooltip("Seconds flying outwards at duration 1.")]
        [SerializeField] private float outwardTime = 0.5f;
        [SerializeField] private float hitRadius = 0.4f;
        [SerializeField] private float aimRange = 8f;
        [Tooltip("Degrees between boomerangs when throwing more than one.")]
        [SerializeField] private float spreadAngle = 25f;
        [SerializeField] private float knockback = 3f;

        public BoomerangWeapon()
        {
            SetDefaults("Boomerang", "Thrown at the nearest enemy, it pierces and returns to you.",
                new Color(1f, 0.65f, 0.3f),
                new WeaponLevelStats("Pierces and comes back.", 12f, 1.8f, 1f, 1),
                new WeaponLevelStats("+33% damage.", 16f, 1.8f, 1f, 1),
                new WeaponLevelStats("+1 boomerang.", 16f, 1.8f, 1f, 2),
                new WeaponLevelStats("Bigger boomerangs.", 16f, 1.8f, 1.3f, 2),
                new WeaponLevelStats("Thrown more often.", 16f, 1.4f, 1.3f, 2),
                new WeaponLevelStats("+1 boomerang.", 18f, 1.4f, 1.3f, 3),
                new WeaponLevelStats("More damage, flies faster and farther.", 24f, 1.4f, 1.3f, 3, 1.3f, 1.2f));
        }

        protected override Sprite DefaultIcon => WeaponVisuals.Boomerang;

        public override WeaponRuntime CreateRuntime(Player player, Transform root) =>
            new Runtime(this, player, root);

        private class Projectile
        {
            public SpriteRenderer Renderer;
            public Vector2 Position;
            public Vector2 Direction;
            public float Elapsed;
            public bool Returning;
            public bool Active;
            public readonly HashSet<Enemy> HitThisPass = new();
        }

        private class Runtime : WeaponRuntime
        {
            private const float CatchDistance = 0.4f;
            private const float MaxFlightTime = 6f;

            private readonly BoomerangWeapon _definition;
            private readonly List<Projectile> _projectiles = new();
            private readonly List<Enemy> _nearby = new();
            private float _timer = 0.3f;

            public Runtime(BoomerangWeapon definition, Player player, Transform root) : base(definition, player, root)
            {
                _definition = definition;
            }

            public override void Tick(float deltaTime)
            {
                _timer -= deltaTime;
                if (_timer <= 0f)
                {
                    _timer = Cooldown;
                    Throw();
                }

                foreach (Projectile projectile in _projectiles)
                {
                    if (projectile.Active) UpdateProjectile(projectile, deltaTime);
                }
            }

            private void Throw()
            {
                Vector2 origin = PlayerPosition;
                Enemy target = FindNearestEnemy(origin, _definition.aimRange);
                Vector2 aim = target ? (target.Position - origin).normalized : Random.insideUnitCircle.normalized;
                if (aim == Vector2.zero) aim = Vector2.right;

                int count = Amount;
                float baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;

                for (int i = 0; i < count; i++)
                {
                    float angle = (baseAngle + (i - (count - 1) * 0.5f) * _definition.spreadAngle) * Mathf.Deg2Rad;

                    Projectile projectile = GetProjectile();
                    projectile.Position = origin;
                    projectile.Direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    projectile.Elapsed = 0f;
                    projectile.Returning = false;
                    projectile.HitThisPass.Clear();
                    projectile.Renderer.transform.position = origin;
                }
            }

            private void UpdateProjectile(Projectile projectile, float deltaTime)
            {
                projectile.Elapsed += deltaTime;
                float speed = _definition.throwSpeed * Speed;

                if (!projectile.Returning)
                {
                    projectile.Position += projectile.Direction * (speed * deltaTime);
                    if (projectile.Elapsed >= _definition.outwardTime * Duration)
                    {
                        projectile.Returning = true;
                        projectile.HitThisPass.Clear();
                    }
                }
                else
                {
                    Vector2 toPlayer = PlayerPosition - projectile.Position;
                    float distance = toPlayer.magnitude;
                    // Comes back a little faster than it left, so it can catch a moving player
                    float step = speed * 1.2f * deltaTime;

                    if (distance <= Mathf.Max(CatchDistance, step) || projectile.Elapsed > MaxFlightTime)
                    {
                        Deactivate(projectile);
                        return;
                    }

                    projectile.Position += toPlayer / distance * step;
                }

                float area = Area;
                Transform t = projectile.Renderer.transform;
                t.position = projectile.Position;
                t.Rotate(0f, 0f, 900f * deltaTime);
                t.localScale = new Vector3(0.8f, 0.8f, 1f) * area;

                CollectEnemies(projectile.Position, _definition.hitRadius * area, _nearby);
                foreach (Enemy enemy in _nearby)
                {
                    if (!projectile.HitThisPass.Add(enemy)) continue;
                    Hit(enemy, 1f, _definition.knockback, projectile.Position);
                }
            }

            private Projectile GetProjectile()
            {
                foreach (Projectile existing in _projectiles)
                {
                    if (existing.Active) continue;
                    existing.Active = true;
                    existing.Renderer.gameObject.SetActive(true);
                    return existing;
                }

                Projectile created = new Projectile
                {
                    Renderer = WeaponVisuals.CreateRenderer("Boomerang", Root, _definition.Icon, _definition.Color, 2),
                    Active = true
                };
                _projectiles.Add(created);
                return created;
            }

            private static void Deactivate(Projectile projectile)
            {
                projectile.Active = false;
                projectile.Renderer.gameObject.SetActive(false);
            }
        }
    }
}
