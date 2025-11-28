using PlayerDirectory;
using UnityEngine;
using Entities.EnemyDirectory;
using Utilities;
using System.Collections;

namespace Classes.WarriorClass
{
    public class Paladin : IClass
    {
        private bool _hasShield;
        private float _shieldEndTime;
        private GameObject _shieldVisual;
        private MultiAttackController _multiAttackController;
        private Player _player;

        public void Initialize(Player player)
        {
            _player = player;
            _multiAttackController = player.gameObject.AddComponent<MultiAttackController>();
        }

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            if (!_multiAttackController.CanAttack()) return;

            int attackCount = 1 + statsMod.projectileCountBonus;
            float intervalBetweenAttacks = 0.08f;

            _player.StartCoroutine(
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

            _player.StartCoroutine(
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

            float damageDealt = CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);

            // Heal 5% of damage dealt
            float healAmount = damageDealt * 0.05f;
            _player.Heal(healAmount);

            ShowHealEffect(vec.Position);
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float radius = playerStats.range * statsMod.rangeMod;

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, radius);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position, playerStats.knockback * statsMod.knockbackMod);

            GrantShield();
        }

        private void GrantShield()
        {
            _hasShield = true;
            _shieldEndTime = Time.time + 10f;

            SpriteRenderer sprite = _player.GetComponent<SpriteRenderer>();
            _player.StartCoroutine(FlashWhite(sprite));
            Debug.Log("Shield activated! Absorbs 1 hit");
        }

        private IEnumerator FlashWhite(SpriteRenderer sprite)
        {
            Color original = sprite.color;
            sprite.color = Color.white;
            yield return new WaitForSeconds(0.2f);
            sprite.color = original;
        }

        private void ShowHealEffect(Vector2 position)
        {
            // TODO: Add heal particle effect
        }

        public bool HasShield()
        {
            if (_hasShield && Time.time > _shieldEndTime)
            {
                _hasShield = false;
            }

            return _hasShield;
        }

        public void ConsumeShield()
        {
            _hasShield = false;
            Debug.Log("Shield consumed!");
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