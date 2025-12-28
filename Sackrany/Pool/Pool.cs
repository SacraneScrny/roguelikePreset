using System;
using System.Collections.Generic;

using Sackrany.Pool.Abstracts;

using UnityEngine;

namespace Sackrany.Pool
{
    public class Pool
    {
        private GameObject _prefab;
        private Stack<IPoolable> _pool = new();
        private Transform _parent;
        
        public void Initialize(GameObject prefab, int prewarm = 0)
        {
            _prefab = prefab;
            CreateParent();

            for (int i = 0; i < prewarm; i++)
                _pool.Push(Create());
        }
        private void CreateParent()
        {
            var allPools = GameObject.Find("Pools");
            if (allPools == null)
                allPools = new GameObject("Pools");
            _parent = new GameObject($"{_prefab.name}_Pool").transform;
            _parent.SetParent(allPools.transform);
        }
        public IPoolable Get()
        {
            IPoolable obj = _pool.Count > 0 ? _pool.Pop() : Create();
            
            obj.gameObject.transform.SetParent(null);
            obj.OnPooled();
            return obj;
        }
        public IPoolable Get(Transform parent)
        {
            IPoolable obj = _pool.Count > 0 ? _pool.Pop() : Create();
            
            obj.gameObject.transform.SetParent(parent);
            obj.OnPooled();
            return obj;
        }

        public void Release(IPoolable obj)
        {
            obj.OnReleased();
            _pool.Push(obj);
            obj.gameObject.transform.SetParent(_parent);
        }
        private IPoolable Create()
        {
            var go = GameObject.Instantiate(_prefab);
            go.name = _prefab.name;
            var poolable = go.GetComponent<IPoolable>();

            go.SetActive(false);
            return poolable;
        }
    }
}