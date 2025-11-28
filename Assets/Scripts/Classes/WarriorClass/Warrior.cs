using System.Collections;
using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace Classes.WarriorClass
{
    public class Warrior : IClass
    {
        private MultiAttackController _multiAttackController;
        private Player _player;
        private Coroutine _currentAttackCoroutine;

        public void Initialize(Player player)
        {
            _player = player;
            _multiAttackController = player.GetComponent<MultiAttackController>();
            if (!_multiAttackController)
            {
                _multiAttackController = player.gameObject.AddComponent<MultiAttackController>();
            }
        }

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            if (!_multiAttackController.CanAttack()) return;

            int attackCount = 1 + statsMod.projectileCountBonus;
            float intervalBetweenAttacks = 0.08f;

            _currentAttackCoroutine = _player.StartCoroutine(
                _multiAttackController.ExecuteMultiAttack(
                    (attackIndex, angleOffset) => PerformSingleBasicAttack(playerStats, statsMod, vec, angleOffset),
                    attackCount,
                    intervalBetweenAttacks
                )
            );
        }

        private void PerformSingleBasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec,
            float angleOffset)
        {
            float range = playerStats.range * statsMod.rangeMod;
            float area = playerStats.area * statsMod.areaMod;

            // Apply angle offset to direction
            Vector2 direction = RotateVector(vec.Direction, angleOffset * 15f);

            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(vec.Position, direction, range, area);

            CombatEvents.TriggerAttack(vec.Position, direction, range, area, AttackType.WarriorBasic);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerStats.knockback * 0.2f * statsMod.knockbackMod);
            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            if (!_multiAttackController.CanAttack()) return;

            int attackCount = 1 + Mathf.FloorToInt(statsMod.projectileCountBonus * 0.5f);
            float intervalBetweenAttacks = 0.12f;

            _currentAttackCoroutine = _player.StartCoroutine(
                _multiAttackController.ExecuteMultiAttack(
                    (attackIndex, angleOffset) => PerformSingleSpecialAttack(playerStats, statsMod, vec, angleOffset),
                    attackCount,
                    intervalBetweenAttacks
                )
            );
        }

        private void PerformSingleSpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec,
            float angleOffset)
        {
            float radius = playerStats.range * statsMod.rangeMod * (1f + angleOffset * 0.1f);

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, radius);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorSpecial);

            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float radius = playerStats.range * statsMod.rangeMod;

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, radius);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position, playerStats.knockback * statsMod.knockbackMod);
        }

        private Vector2 RotateVector(Vector2 vector, float degrees)
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