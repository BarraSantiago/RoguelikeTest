using System.Collections.Generic;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace Classes.RangerClass
{
    public class Trapper : IClass
    {
        public ArrowPool ArrowPool;
        private List<GameObject> _activeTraps = new List<GameObject>();
        private const int MaxTraps = 3;

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject arrow = ArrowPool.GetArrow();
            Arrow arrowScript = arrow.GetComponent<Arrow>();

            arrowScript.damage = CombatUtilities.CalculateDamage(playerStats, statsMod);
            arrowScript.range = playerStats.range * statsMod.rangeMod;
            arrowScript.leaveTrap = true;
            arrowScript.trapDuration = 3f;
            arrowScript.trapRadius = 2f;
            arrowScript.trapperReference = this;

            arrow.transform.position = vec.Position;
            arrow.GetComponent<Rigidbody2D>().linearVelocity =
                vec.Direction.normalized * playerStats.projectileSpeed * statsMod.projectileSpeedMod;
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            // Create root trap at position
            float rootRadius = playerStats.range * statsMod.rangeMod * 0.4f;
            GameObject trap = new GameObject("RootTrap");
            trap.transform.position = vec.Position;

            TrapZone trapZone = trap.AddComponent<TrapZone>();
            trapZone.Initialize(rootRadius, 3f, true); // Rooting trap

            RegisterTrap(trap);
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector2 dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

            if (dodgeDirection == Vector2.zero)
                dodgeDirection = -vec.Direction;

            player.transform.position += (Vector3)dodgeDirection * playerStats.speed * 2f * statsMod.speedMod;

            // Leave trap at dodge start position
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