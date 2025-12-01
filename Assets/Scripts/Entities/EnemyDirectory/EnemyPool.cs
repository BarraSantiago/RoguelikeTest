using UnityEngine;

namespace Entities.EnemyDirectory
{
    public class EnemyPool : GenericPool<Enemy>
    {
        public void Initialize(GameObject prefab, int initialSize, Transform parent)
        {
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Object.Instantiate(prefab, parent);
                Enemy enemy = obj.GetComponent<Enemy>();
                obj.SetActive(false);
                Return(enemy);
            }
        }
    }
}