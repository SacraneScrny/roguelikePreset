using System;
using System.Collections.Generic;
using System.Linq;

using Sackrany.SerializableData.Entities;

using UnityEngine;

namespace Sackrany.SerializableData.Components
{
    public abstract class SerializableBehaviour : MonoBehaviour
    {
        public string Guid;
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(Guid))
                Guid = System.Guid.NewGuid().ToString();
        }
        public void ForceUpdateGuid() => Guid = System.Guid.NewGuid().ToString();
        
        private bool _isLoaded = false;
        private protected bool IsLoaded => _isLoaded;
        public void MarkAsLoaded() => _isLoaded = true;
        
        public Dictionary<string, object> _serializedFields = new ();
        
        private Dictionary<string, ISerialize> _serializeRules = new ();

        private void Awake()
        {
            Register();
            DataManager.RegisterSerializable(this);
            OnAwake();
        }
        private protected virtual void OnAwake() {}
        
        public void Register()
        {
            _serializeRules.Clear();
            OnRegister();
        }
        private protected abstract void OnRegister();
        private protected void RegisterSerializable<T>(string key, Func<T> _get, Action<T> _set)
            => _serializeRules.TryAdd(key, new SerializeEntity<T>(_get, _set));

        public void Serialize()
        {
            _serializedFields = _serializeRules
                .ToDictionary(x => x.Key, x => x.Value.Get());
        }
        public void Deserialize(Dictionary<string, object> _cache)
        {
            foreach (var rule in _serializeRules)
                if (_cache.ContainsKey(rule.Key))
                    rule.Value.Set(_cache[rule.Key]);
            MarkAsLoaded();
        }
    }
}