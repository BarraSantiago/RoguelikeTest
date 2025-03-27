using UnityEngine;
using PlayerDirectory;
using Entities.EnemyDirectory;
using Classes.RangerClass;
using Utilities;

namespace PowerUps
{
    [CreateAssetMenu(fileName = "BlueArrowPowerUp", menuName = "PowerUps/BlueArrow")]
    public class BlueArrowPowerUp : PowerUp
    {
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private Color arrowColor = Color.blue;
        
        private ArrowPool arrowPool;

        public override void OnPickup(Player player)
        {
            base.OnPickup(player);
            if (player.gameObject.GetComponent<ArrowPool>())
            {
                arrowPool = player.gameObject.GetComponent<ArrowPool>();
                return;
            }

            arrowPool = player.gameObject.AddComponent<ArrowPool>();
        }

        public override void Activate(Player player)
        {
            base.Activate(player);
            if(!CanActivate()) return;
            Enemy closestEnemy = CombatUtilities.GetClosestEnemy(player.transform.position, detectionRadius);
            if (!closestEnemy) return;

            if (!arrowPool) return;

            GameObject arrowObj = arrowPool.GetArrow();

            Arrow arrow = arrowObj.GetComponent<Arrow>();
            arrow.damage = CombatUtilities.CalculateDamage(PowerUpStats, PowerUpStatsMod);
            arrow.range = player.playerStats.range;
            arrow.isPiercing = false;

            SpriteRenderer spriteRenderer = arrowObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                spriteRenderer.color = arrowColor;
            }

            arrowObj.transform.position = player.transform.position;
            Vector2 direction = (closestEnemy.transform.position - player.transform.position).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            Rigidbody2D rb = arrowObj.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.linearVelocity = direction * player.playerStats.projectileSpeed;
            }

            arrowObj.SetActive(true);
        }
    }
}