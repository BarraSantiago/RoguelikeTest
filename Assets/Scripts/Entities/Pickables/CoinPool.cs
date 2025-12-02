using UnityEngine;

namespace Entities.Pickables
{
    public class CoinPool : GenericPool<CoinDrop>
    {
        public void SpawnCoinDrop(Vector3 position, float coinValue)
        {
            CoinDrop coin = Get();
            coin.Initialize(coinValue);
            coin.transform.position = position;
        }
    }
}