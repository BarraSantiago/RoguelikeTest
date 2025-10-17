using UnityEngine;

namespace Entities.EnemyDirectory
{
    public class EnemyFactory
    {
        private EnemyPool _enemyPool;

        public EnemyFactory(EnemyPool enemyPool)
        {
            this._enemyPool = enemyPool;
        }

        public IEnemy CreateEnemy(Vector3 position)
        {
            IEnemy enemyObject = _enemyPool.Get();
            enemyObject.SetPosition(position);
            return enemyObject;
        }
    }
}