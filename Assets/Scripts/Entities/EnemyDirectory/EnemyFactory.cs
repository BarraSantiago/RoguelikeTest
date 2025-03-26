using UnityEngine;

namespace Entities.EnemyDirectory
{
    public class EnemyFactory
    {
        private EnemyPool enemyPool;

        public EnemyFactory(EnemyPool enemyPool)
        {
            this.enemyPool = enemyPool;
        }

        public IEnemy CreateEnemy(Vector3 position)
        {
            GameObject enemyObject = enemyPool.GetEnemy();
            enemyObject.transform.position = position;
            return enemyObject.GetComponent<IEnemy>();
        }
    }
}