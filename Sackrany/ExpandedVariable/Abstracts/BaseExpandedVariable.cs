using System;

using UnityEngine;

namespace Sackrany.ExpandedVariable.Abstracts
{
    [Serializable]
    public abstract class BaseExpandedVariable<T>
    {
        public delegate T expandedDelegate();
        private bool _hasInited = false;
        private T _defaultValue;
        [SerializeField] private protected T Variable;
        
        protected BaseExpandedVariable(T value)
        {
            Variable = value;
        }

        #if UNITY_EDITOR
        public T GetValueEditor()
        {
            T result = Value();
            return result;
        }
        #endif
        
        public T GetValue()
        {
            if (!_hasInited)
            {
                _defaultValue = Variable;
                _hasInited = true;
            }
            T result = Value();
            OnValueChanged?.Invoke(result);
            return result;
        }
        public void SetOriginalValue(T value)
        {
            Variable = value;
            T result = Value();
            OnValueChanged?.Invoke(result);
        }
        public T GetOriginalValue() => Variable; 
        
        private protected abstract T CalculateValue();
        private protected T Value()
        {
            Preload();
            return CalculateValue();
        }

        private protected abstract void Preload();

        public void Clear()
        {
            OnValueChanged = null;
            if (_hasInited)
                Variable = _defaultValue;
            OnClear();
        }
        private protected abstract void OnClear();
        public abstract object Clone();

        public static implicit operator T (BaseExpandedVariable<T> obj)
        {
            return obj.GetValue();
        }
        
        public System.Action<T> OnValueChanged;
    }
}