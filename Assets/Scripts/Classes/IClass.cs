using PlayerDirectory;

namespace Classes
{
    public interface IClass
    {
        public void BasicAttack(PlayerStats playerStats, Direction direction);
        public void SpecialAttack(PlayerStats playerStats, Direction direction);
        public void Defense(PlayerStats playerStats, Direction direction);
    }
}