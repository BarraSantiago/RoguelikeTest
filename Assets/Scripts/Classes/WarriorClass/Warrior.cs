using Entities.EnemyDirectory;
using PlayerDirectory;
using Utilities;

namespace Classes.WarriorClass
{
    public class Warrior : IClass
    {
        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            float range = playerStats.range * statsMod.rangeMod;
            float area = playerStats.area * statsMod.areaMod;
            
            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(direction.position, direction.direction,
                playerStats.range * statsMod.rangeMod, playerStats.area * statsMod.areaMod);

            CombatEvents.TriggerAttack(direction.position, direction.direction, range, area, AttackType.WarriorBasic);

            CombatUtilities.KnockbackEnemies(enemies, direction.position, playerStats.knockback * 0.2f * statsMod.knockbackMod);
            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            float radius = playerStats.range * statsMod.rangeMod;

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(direction.position, playerStats.range * statsMod.rangeMod);
            CombatEvents.TriggerRadialAttack(direction.position, radius, AttackType.WarriorSpecial);

            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            float radius = playerStats.range * statsMod.rangeMod;

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(direction.position, playerStats.range * statsMod.rangeMod);
            CombatEvents.TriggerRadialAttack(direction.position, radius, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, direction.position, playerStats.knockback * statsMod.knockbackMod);
        }
    }
}