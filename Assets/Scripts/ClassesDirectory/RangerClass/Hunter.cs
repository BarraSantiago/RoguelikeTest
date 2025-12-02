using System.Collections;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace ClassesDirectory.RangerClass
{
    public class Hunter : RangerBase
    {
        private const float SpreadAngle = 15f;

        public override void BasicAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerBaseStats, baseStatsMod);
            float range = playerBaseStats.Range * baseStatsMod.rangeMod;
            float speed = playerBaseStats.ProjectileSpeed * baseStatsMod.projectileSpeedMod;
            int projectileCount = GetProjectileCount(playerBaseStats, baseStatsMod);

            ArrowPool.StartCoroutine(FireArrowsSequential(vec.Position, vec.Direction, speed, damage, range,
                projectileCount, SpreadAngle, arrow =>
                {
                    arrow.isHoming = true;
                    arrow.slowChance = 0.3f;
                    arrow.slowDuration = 2f;
                }));
        }

        public override void SpecialAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerBaseStats, baseStatsMod, 0.8f);
            float range = playerBaseStats.Range * baseStatsMod.rangeMod;
            float speed = playerBaseStats.ProjectileSpeed * baseStatsMod.projectileSpeedMod;

            ArrowPool.StartCoroutine(FireThreeWayAttack(vec.Position, vec.Direction, speed, damage, range));
        }

        public override void Defense(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            PerformDodge(2.5f, playerBaseStats, baseStatsMod, vec);
        }

        private IEnumerator FireThreeWayAttack(Vector2 position, Vector2 direction, float speed, float damage,
            float range)
        {
            Vector2[] directions = new Vector2[3];
            directions[0] = direction;
            directions[1] = Quaternion.Euler(0, 0, 30f) * direction;
            directions[2] = Quaternion.Euler(0, 0, -30f) * direction;

            for (int i = 0; i < 3; i++)
            {
                Vector2 velocity = directions[i].normalized * speed;
                Arrow arrow = ArrowPool.GetArrowWithSetup(position, velocity, damage, range);
                arrow.isHoming = true;

                if (i < 2)
                {
                    yield return new WaitForSeconds(ProjectileDelay);
                }
            }
        }
    }
}