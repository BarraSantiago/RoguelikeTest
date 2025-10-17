using PlayerDirectory;

namespace Classes
{
    public interface IClass
    {
        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec);
        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec);
        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec);
    }
}