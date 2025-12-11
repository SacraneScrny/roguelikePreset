using System;

using Sackrany.Unit.Abstracts;
using Sackrany.Unit.Base;

namespace Sackrany.Unit.ModuleSystem.Main
{
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

        public void Awake(UnitBase unit)
        {
            Unit = unit;
            OnAwake(unit);
            _isAwaken = true;
        }
        protected virtual void OnAwake(UnitBase unit) { }

        public virtual bool DependencyCheck() => true;

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
        protected TSelf Get<T>() where T : TSelf
            => _connector.Get<T>();
        protected bool TryGet<T>(out T module) where T : TSelf
            => _connector.TryGet(out module);
        
        protected bool HasOther<T>() where T : class
            => _connector != null && _connector.HasOther<T>();
        protected bool HasOther<T>(T value) where T : class
            => _connector != null && _connector.HasOther<T>(value);
        protected T GetOther<T>() where T : class
            => _connector.GetOther<T>();
        protected bool GetOther<T>(out T value) where T : class
            => _connector.GetOther<T>(out value);
    }

    public abstract class LinkedModule<TSelf, TController> : Module<TSelf>
        where TSelf : ModuleBase, new()
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

    public class VController : ModuleController<VModule, VTemplate>
    {
        
    }
    public class VModule : LinkedModule<VModule, VController>
    {
        public VModule() : base()
        {
            
        }
    }
    public class VTemplate : TemplateBase<VModule>
    {
        
    }
}