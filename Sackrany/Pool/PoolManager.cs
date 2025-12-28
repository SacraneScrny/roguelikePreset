using System.Collections.Generic;

using Sackrany.Hash;
using Sackrany.Utils;

using UnityEngine;

namespace Sackrany.Pool
{
    public class PoolManager : AManager<PoolManager>
    {
        private readonly Dictionary<uint, Pool> pools = new Dictionary<uint, Pool>();

        public static Pool GetOrCreatePool(GameObject go)
        {
            var hash = go.name.XXHash();
            if (Instance.pools.TryGetValue(hash, out var pool))
                return pool;
            var createdPool = new Pool();
            createdPool.Initialize(go);
            Instance.pools.Add(hash, createdPool);
            return createdPool;
        }
    }
}