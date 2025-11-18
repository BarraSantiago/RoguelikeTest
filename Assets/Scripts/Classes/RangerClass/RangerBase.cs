using PlayerDirectory;
using System.Collections;
using UnityEngine;
using Utilities;

namespace Classes.RangerClass
{
    public abstract class RangerBase : IClass
    {
        public ArrowPool ArrowPool;
        protected const float ProjectileDelay = 0.1f;

        public abstract void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec);
        public abstract void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec);
        public abstract void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec);

        protected int GetProjectileCount(PlayerStats playerStats, PlayerStatsModifier statsMod, bool applyLuck = true)
        {
            int count = playerStats.projectileCount + statsMod.projectileCountBonus;

            if (applyLuck && CombatUtilities.RollLuck(playerStats.luck, statsMod.luckMod))
            {
                count++;
            }

            return count;
        }

        protected IEnumerator FireArrowsSequential(Vector2 position, Vector2 direction, float speed, float damage,
            float range, int count, float spreadAngle, System.Action<Arrow> configureArrow = null)
        {
            float startAngle = -(count - 1) * spreadAngle * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float currentAngle = startAngle + (i * spreadAngle);
                Vector2 rotatedDirection = RotateVector(direction, currentAngle);
                Vector2 velocity = rotatedDirection.normalized * speed;

                Arrow arrow = ArrowPool.GetArrowWithSetup(position, velocity, damage, range);
                configureArrow?.Invoke(arrow);

                if (i < count - 1)
                {
                    yield return new WaitForSeconds(ProjectileDelay);
                }
            }
        }

        protected void PerformDodge(float speedMultiplier, PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (!player) return;

            Vector2 dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

            if (dodgeDirection == Vector2.zero)
            {
                dodgeDirection = vec.Direction != Vector2.zero ? -vec.Direction : Vector2.right;
            }

            player.transform.position += (Vector3)dodgeDirection * playerStats.speed * speedMultiplier * statsMod.speedMod;
        }

        protected Vector2 RotateVector(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }
    }
}