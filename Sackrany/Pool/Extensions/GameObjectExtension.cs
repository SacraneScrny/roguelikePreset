using Sackrany.Pool.Abstracts;

using UnityEngine;

namespace Sackrany.Pool.Extensions
{
    public static class GameObjectExtension
    {
        public static GameObject POOL(this GameObject gameObject)
        {
            var pool = PoolManager.GetOrCreatePool(gameObject);
            return pool.Get().gameObject;
        }
        public static GameObject POOL(this GameObject gameObject, Transform parent)
        {
            var pool = PoolManager.GetOrCreatePool(gameObject);
            return pool.Get(parent).gameObject;
        }
        public static void RELEASE(this GameObject gameObject)
        {
            var pool = PoolManager.GetOrCreatePool(gameObject);
            if (gameObject.TryGetComponent(out IPoolable poolable))
                pool.Release(poolable);
            else
                GameObject.Destroy(gameObject);
        }
        public static void RELEASE(this IPoolable poolable)
        {
            var pool = PoolManager.GetOrCreatePool(poolable.gameObject);
            pool.Release(poolable);
        }
    }
}