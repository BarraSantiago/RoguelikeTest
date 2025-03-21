using PlayerDirectory;

namespace Classes
{
    public interface IClass
    {
        public void BasicAttack(PlayerStats playerStats);
        public void SpecialAttack(PlayerStats playerStats);
        public void Defense(PlayerStats playerStats);
    }
}