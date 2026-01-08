using System.Collections.Generic;

using Sackrany.Pool.Abstracts;
using Sackrany.Utils;

using UnityEngine;

namespace Sackrany.Pool
{
    public class PoolManager : AManager<PoolManager>
    {
        private readonly Dictionary<uint, Pool> pools = new Dictionary<uint, Pool>();

        public static Pool GetOrCreatePool(GameObject go, uint hash)
        {
            if (Instance.pools.TryGetValue(hash, out var pool))
                return pool;
            var createdPool = new Pool();
            createdPool.Initialize(new PoolItem(go));
            Instance.pools.Add(hash, createdPool);
            return createdPool;
        }
    }
}