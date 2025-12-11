namespace Sackrany.ExpandedVariable.Abstracts
{
    public abstract class BaseExpandedVariable<T>
    {
        public delegate T expandedDelegate();
        private protected readonly T defaultVariable;
        private protected T Variable;
        
        protected BaseExpandedVariable(T value)
        {
            defaultVariable = value;
            Variable = defaultVariable;
        }
        
        public T GetValue()
        {
            T result = Value();
            OnValueChanged(result);
            return result;
        }
        public void SetValue(T value)
        {
            Variable = value;
            OnValueChanged(value);
        }
        
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
            Variable = defaultVariable;
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