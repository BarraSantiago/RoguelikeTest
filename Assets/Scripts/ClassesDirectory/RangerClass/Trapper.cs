using System.Collections.Generic;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace ClassesDirectory.RangerClass
{
    public class Trapper : RangerBase
    {
        private const float SpreadAngle = 15f;
        private List<GameObject> _activeTraps = new List<GameObject>();
        private const int MaxTraps = 3;

        public override void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod);
            float range = playerStats.Range * statsMod.rangeMod;
            float speed = playerStats.ProjectileSpeed * statsMod.projectileSpeedMod;
            int projectileCount = GetProjectileCount(playerStats, statsMod);

            ArrowPool.StartCoroutine(FireArrowsSequential(vec.Position, vec.Direction, speed, damage, range,
                projectileCount, SpreadAngle, arrow =>
                {
                    arrow.leaveTrap = true;
                    arrow.trapDuration = 3f;
                    arrow.trapRadius = 2f;
                    arrow.trapperReference = this;
                }));
        }

        public override void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float rootRadius = playerStats.Range * statsMod.rangeMod * 0.4f;
            GameObject trap = new GameObject("RootTrap");
            trap.transform.position = vec.Position;

            TrapZone trapZone = trap.AddComponent<TrapZone>();
            trapZone.Initialize(rootRadius, 3f, true);

            RegisterTrap(trap);
        }

        public override void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            PerformDodge(2f, playerStats, statsMod, vec);

            GameObject trap = new GameObject("DodgeTrap");
            trap.transform.position = vec.Position;

            TrapZone trapZone = trap.AddComponent<TrapZone>();
            trapZone.Initialize(2f, 3f, false);

            RegisterTrap(trap);
        }

        public void RegisterTrap(GameObject trap)
        {
            if (_activeTraps.Count >= MaxTraps)
            {
                GameObject oldest = _activeTraps[0];
                _activeTraps.RemoveAt(0);
                Object.Destroy(oldest);
            }

            _activeTraps.Add(trap);
        }

        public void RemoveTrap(GameObject trap)
        {
            _activeTraps.Remove(trap);
        }
    }
}