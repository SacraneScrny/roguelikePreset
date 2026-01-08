using System;
using System.Reflection;

using Sackrany.Unit.Abstracts;
using Sackrany.Unit.Base;

using UnityEngine;

namespace Sackrany.Unit.ModuleSystem.Main
{
    [Serializable] 
    public abstract class ModuleBase : IDisposable
    {
        private protected UnitBase Unit;
        
        private bool _isAwaken;
        private bool _isStarted;
        private bool _isDisposed;
        
        public bool IsAwaken => _isAwaken;
        public bool IsStarted => _isStarted;
        public bool IsDisposed => _isDisposed;
        
        protected ModuleBase() { }
        protected ModuleBase(ITemplate<ModuleBase> template) { }

        public void UnitFill(UnitBase unit)
        {
            Unit = unit;
        }
        public void Awake()
        {
            OnAwake();
            _isAwaken = true;
        }
        protected virtual void OnAwake() { }

        public bool DependencyCheck()
        {
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<DependencyAttribute>();
                if (attr == null) continue;

                if (!TryResolveDependencies(field.FieldType, out var module))
                {
                    if (attr.Optional) continue;
                    Debug.Log($"Module {GetType().Name} failed to resolve {field.FieldType.Name} dependency");
                    return false;
                }
                field.SetValue(this, module);
            }
            return OnDependencyCheck();
        }
        private protected virtual bool OnDependencyCheck() => true;
        private bool TryResolveDependencies(Type type, out object result)
        {
            if (Unit.TryGetModule(type, out var module))
            {
                result = module;
                return true;
            }

            if (typeof(Component).IsAssignableFrom(type))
            {
                var comp = Unit.GetComponent(type);
                if (comp != null)
                {
                    result = comp;
                    return true;
                }
                comp = Unit.GetComponentInChildren(type);
                if (comp != null)
                {
                    result = comp;
                    return true;
                }
            }

            result = null;
            return false;
        }
        public void TemplateFill(object template)
        {
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<TemplateAttribute>();
                if (attr == null) continue;

                if (field.FieldType == template.GetType())
                    field.SetValue(this, template);
                else
                    Debug.Log($"Template {GetType().Name} failed to fill {field.FieldType.Name} dependency");
            }
        }
        
        public void Start()
        {
            OnStart();
            _isStarted = true;
        }
        protected virtual void OnStart() { }
        
        public bool IsInitialized()
            => _isAwaken && _isStarted && !_isDisposed;
        
        public void Reset()
        {
            
        }
        protected virtual void OnReset() { }
        
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            if (!_isAwaken) return;
            if (!_isStarted) return;
            OnDispose();
        }
        protected virtual void OnDispose() { }
    }

    [Serializable] 
    public abstract class Module<TSelf> : ModuleBase
        where TSelf : ModuleBase, new()
    {
        private IModuleConnector<TSelf> _connector;

        public virtual void SetConnector(IModuleConnector<TSelf> connector)
        {
            _connector = connector;
        }
        
        protected bool Add<TTemplate>(TTemplate template) where TTemplate : ITemplate<TSelf>
            => _connector != null && _connector.Add(template);
        protected bool Add<TTemplate>(TTemplate[] templates) where TTemplate : ITemplate<TSelf>
            => _connector != null && _connector.Add(templates);

        protected bool Remove<T>() where T : TSelf
            => _connector != null && _connector.Remove<T>();
        protected bool Remove(params Type[] types)
            => _connector != null && _connector.Remove(types);
        
        protected bool Has<T>() where T : TSelf
            => _connector != null && _connector.Has<T>();
        protected bool Has<T>(T value) where T : TSelf
            => _connector != null && _connector.Has(value);
        protected TSelf Get<K>(K template) where K : ITemplate<TSelf>
            => _connector.Get(template);
        protected T Get<T>() where T : TSelf
            => _connector.Get<T>();
        protected bool TryGet<T>(out T module) where T : ModuleBase
            => _connector.TryGet(out module);
        protected bool TryGet(Type t, out ModuleBase module)
            => _connector.TryGet(t, out module);
        
        protected bool HasOther<T>() where T : class
            => _connector != null && _connector.HasOther<T>();
        protected bool HasOther<T>(T value) where T : class
            => _connector != null && _connector.HasOther<T>(value);
        protected T GetOther<T>() where T : class
            => _connector.GetOther<T>();
        protected bool GetOther<T>(out T value) where T : class
            => _connector.GetOther<T>(out value);
    }

    [Serializable] 
    public abstract class LinkedModule<TSelf, TController> : Module<TSelf>
        where TSelf : ModuleBase, new ()
        where TController : class, IModuleConnector<TSelf>
    {
        public TController Controller { get; private set; }
        
        public override void SetConnector(IModuleConnector<TSelf> connector)
        {
            base.SetConnector(connector);
            
            if (connector is TController specificController)
            {
                Controller = specificController;
            }
            else
            {
                UnityEngine.Debug.LogError($"[LinkedModule] Module {GetType().Name} expected controller of type {typeof(TController).Name}, but got {connector?.GetType().Name ?? "null"}");
            }
        }
    }

    public interface ISerializableModule
    {
        public object[] Serialize();
        public void Deserialize(object[] data);
    }
}