using Sackrany.Hash;
using Sackrany.Pool.Abstracts;
using Sackrany.Unit.Abstracts;

using UnityEngine;

namespace Sackrany.Pool.Extensions
{
    public static class PoolExtensions
    {
        public static GameObject POOL(this GameObject gameObject, Transform parent = null)
        {
            var pool = PoolManager.GetOrCreatePool(gameObject, gameObject.name.XXHash());
            return pool.Get(parent).gameObject;
        }
        public static UnitBase POOL(this UnitBase unit, Transform parent = null)
        {
            var pool = PoolManager.GetOrCreatePool(unit.gameObject, unit.GetArchetype().Hash);
            return pool.Get(parent).gameObject.GetComponent<UnitBase>();
        }
        
        public static void RELEASE(this GameObject gameObject)
        {
            var pool = PoolManager.GetOrCreatePool(gameObject, gameObject.name.XXHash());
            if (gameObject.TryGetComponent(out IPoolable poolable))
                pool.Release(poolable);
            else
                GameObject.Destroy(gameObject);
        }
        public static void RELEASE(this IPoolable poolable)
        {
            var pool = PoolManager.GetOrCreatePool(poolable.gameObject, poolable.gameObject.name.XXHash());
            pool.Release(poolable);
        }
        public static void RELEASE(this UnitBase unit)
        {
            var pool = PoolManager.GetOrCreatePool(unit.gameObject, unit.GetArchetype().Hash);
            pool.Release(unit);
        }
    }
}