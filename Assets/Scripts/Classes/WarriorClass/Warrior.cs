using Entities.EnemyDirectory;
using PlayerDirectory;
using Utilities;

namespace Classes.WarriorClass
{
    public class Warrior : IClass
    {
        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(direction.position, direction.direction,
                playerStats.range * statsMod.rangeMod, playerStats.area * statsMod.areaMod);

            CombatUtilities.KnockbackEnemies(enemies, direction.position,
                playerStats.knockback * 0.2f * statsMod.knockbackMod);
            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            Enemy[] enemies =
                CombatUtilities.GetEnemiesInRadius(direction.position, playerStats.range * statsMod.rangeMod);
            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            Enemy[] enemies =
                CombatUtilities.GetEnemiesInRadius(direction.position, playerStats.range * statsMod.rangeMod);
            CombatUtilities.KnockbackEnemies(enemies, direction.position,
                playerStats.knockback * statsMod.knockbackMod);
        }
    }
}