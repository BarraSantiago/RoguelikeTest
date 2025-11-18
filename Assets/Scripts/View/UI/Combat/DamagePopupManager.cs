using Entities.EnemyDirectory;
using UnityEngine;

namespace View.UI.Combat
{
    public class DamagePopupManager : MonoBehaviour
    {
        private DamagePopupPool _popupPool;

        private void Awake()
        {
            _popupPool = GetComponent<DamagePopupPool>();
        }

        private void OnEnable()
        {
            Enemy.OnEnemyDamaged += HandleEnemyDamaged;
        }

        private void OnDisable()
        {
            Enemy.OnEnemyDamaged -= HandleEnemyDamaged;
        }

        private void HandleEnemyDamaged(Enemy enemy, float damage, bool isCritical)
        {
            if (enemy && enemy.gameObject.activeInHierarchy)
            {
                Vector3 popupPosition = enemy.Position + Vector2.up * 0.5f;
                _popupPool.ShowDamage(popupPosition, damage, isCritical);
            }
        }
    }
}