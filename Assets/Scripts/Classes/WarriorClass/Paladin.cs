using PlayerDirectory;
using UnityEngine;
using Entities.EnemyDirectory;
using Utilities;

namespace Classes.WarriorClass
{
    public class Paladin : IClass
    {
        private bool _hasShield;
        private float _shieldEndTime;
        private GameObject _shieldVisual;

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(vec.Position, vec.Direction,
                playerStats.range * statsMod.rangeMod, playerStats.area * statsMod.areaMod);

            CombatEvents.TriggerAttack(vec.Position, vec.Direction,
                playerStats.range * statsMod.rangeMod, playerStats.area * statsMod.areaMod,
                AttackType.WarriorBasic);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerStats.knockback * 0.2f * statsMod.knockbackMod);
            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position,
                playerStats.range * statsMod.rangeMod);

            CombatEvents.TriggerRadialAttack(vec.Position,
                playerStats.range * statsMod.rangeMod, AttackType.WarriorSpecial);

            float damageDealt = CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);

            // Heal 5% of damage dealt
            float healAmount = damageDealt * 0.05f;
            GameObject.FindWithTag("Player").GetComponent<Player>().Heal(healAmount);

            // Visual feedback
            ShowHealEffect(vec.Position);
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position,
                playerStats.range * statsMod.rangeMod);

            CombatEvents.TriggerRadialAttack(vec.Position,
                playerStats.range * statsMod.rangeMod, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerStats.knockback * statsMod.knockbackMod);

            // Grant shield
            GrantShield();
        }

        private void GrantShield()
        {
            _hasShield = true;
            _shieldEndTime = Time.time + 10f;

            // Visual feedback - flash white
            Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
            SpriteRenderer sprite = player.GetComponent<SpriteRenderer>();

            player.StartCoroutine(FlashWhite(sprite));
            Debug.Log("Shield activated! Absorbs 1 hit");
        }

        private System.Collections.IEnumerator FlashWhite(SpriteRenderer sprite)
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
    }
}