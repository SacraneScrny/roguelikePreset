using System;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Sackrany.Utils
{
    public abstract class AManager<T> : MonoBehaviour where T : AManager<T>
    {
        private protected static T _instance;
        
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<T>(FindObjectsInactive.Exclude);

                    if (_instance == null)
                    {
                        var go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            gameObject.name = typeof(T).Name;
        }
        #endif
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;
            OnManagerAwake();
        }
        private protected virtual void OnManagerAwake() { }

        private void OnDestroy()
        {
            OnManagerDestroy();
            if (_instance == this)
                _instance = null;
        }
        private protected virtual void OnManagerDestroy() { }
    }
}