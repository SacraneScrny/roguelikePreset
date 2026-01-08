using System;
using System.Collections.Generic;

using Sackrany.Extensions;
using Sackrany.Unit.Abstracts;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

namespace Sackrany.Unit.ModuleSystem.Main
{
    [Serializable]
    public abstract class ModuleController<TModule, TTemplate>
        : IModuleConnector<TModule>, IModuleController
        where TModule : ModuleBase, new()
        where TTemplate : ITemplate<TModule>
    {
        [SerializeField][SerializeReference][SubclassSelector] 
        public TTemplate[] Default;
        
        private protected readonly Dictionary<Type, TModule> _modules = new ();
        public IEnumerable<ModuleBase> GetModules() => _modules.Values;
        public bool TryGetModule<T>(out T module) where T : ModuleBase
        {
            if (_modules.TryGetValue(typeof(T), out var m))
            {
                module = m as T;
                return true;
            }
            module = null;
            return false;
        }
        public bool TryGetModule(Type type, out ModuleBase module)
        {
            if (_modules.TryGetValue(type, out var m))
            {
                module = m;
                return true;
            }
            module = null;
            return false;
        }
        public bool HasModule<T>() where T : ModuleBase => _modules.ContainsKey(typeof(T));
        
        private protected UnitBase CurrentUnit;
        
        public void Initialize(UnitBase unit)
        {
            CurrentUnit = unit;
            Add(Default);
            OnInitialize(unit);
        }
        protected virtual void OnInitialize(UnitBase unit) { }
        
        protected virtual void AddedModulePostprocess(TModule module) { }
        protected virtual void RemovedModulePostprocess(TModule module) { }
        
        public bool Add<T>(T template) where T : TTemplate
        {
            return AddInternal(template);
        }
        bool IModuleConnector<TModule>.Add<T>(T template)
        {
            return AddInternal((TTemplate)(object)template);
        }
        private protected virtual bool AddInternal(TTemplate template)
        {
            if (_modules.ContainsKey(template.GetModuleType()))
            {
                return Add_AlreadyExist(template);
            }
            var instance = template.GetModuleInstance();
            instance.TemplateFill(template);
            instance.UnitFill(CurrentUnit);
            
            if (instance is Module<TModule> moduleInstance)
            {
                moduleInstance.SetConnector(this);
            }
            
            _modules.Add(instance.GetType(), instance);
            if (!instance.DependencyCheck())
            {
                Debug.Log($"{instance.GetType().Name} failed on dependency check");
                _modules.Remove(instance.GetType());
                return false;
            }
            instance.Awake();
            instance.Start();
            AddedModulePostprocess(instance);
            OnModuleAdded?.Invoke(instance);
            return true;
        }
        private protected virtual bool Add_AlreadyExist<T>(T template) where T : TTemplate => false;
        
        public bool Add<T>(T[] templates) where T : TTemplate
        {
            return AddInternal(templates);
        }
        bool IModuleConnector<TModule>.Add<T>(T[] templates)
        {
            return AddInternal((TTemplate[])(object)templates);
        }
        private protected virtual bool AddInternal<T>(T[] templates) where T : TTemplate
        {
            bool allAdded = true;
            List<TModule> modules = new();
            for (int i = 0; i < templates.Length; i++)
            {
                if (_modules.ContainsKey(templates[i].GetModuleType()))
                {
                    Add_AlreadyExist(templates[i]);
                    continue;
                }
                var instance = templates[i].GetModuleInstance();
                instance.TemplateFill(templates[i]);
                instance.UnitFill(CurrentUnit);
                
                if (instance is Module<TModule> moduleInstance)
                {
                    moduleInstance.SetConnector(this);
                }
                
                _modules.Add(instance.GetType(), instance);
                modules.Add(instance);
            }
            for (int i = modules.Count - 1; i >= 0; i--)
            {
                if (!modules[i].DependencyCheck())
                {
                    Debug.Log($"{modules[i].GetType().Name} failed on dependency check");
                    _modules.Remove(modules[i].GetType());
                    modules.RemoveAt(i);
                    allAdded = false;
                    continue;
                }
                modules[i].Awake();
            }
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].Start();
                AddedModulePostprocess(modules[i]);
                OnModuleAdded?.Invoke(modules[i]);
            }
            return allAdded;
        }

        public virtual bool Remove<T>() where T : TModule
        {
            if (!_modules.ContainsKey(typeof(T))) return false;
            RemovedModulePostprocess(_modules[typeof(T)]);
            OnModuleRemoved?.Invoke(_modules[typeof(T)]);
            _modules.Remove(typeof(T));
            return true;
        }
        public virtual bool Remove(params Type[] types)
        {
            bool allRemoved = true;
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null) continue;
                
                if (!_modules.ContainsKey(types[i]))
                {
                    allRemoved = false;
                    continue;
                }
                RemovedModulePostprocess(_modules[types[i]]);
                OnModuleRemoved?.Invoke(_modules[types[i]]);
                _modules.Remove(types[i]);
            }
            return allRemoved;
        }
        
        public bool Has<T>(T value) where T : TModule
        {
            return _modules.ContainsKey(typeof (T));
        }
        public bool Has<T>() where T : TModule
        {
            return _modules.ContainsKey(typeof (T));
        }
        public T Get<T>() where T : TModule
        {
            return _modules[typeof (T)] as T;
        }
        public TModule Get<K>(K template) where K : ITemplate<TModule>
        {
            return _modules[template.GetModuleType()];
        }
        public bool TryGet<T>(out T value) where T : ModuleBase
        {
            if (_modules.TryGetValue(typeof(T), out var unitComponent))
            {
                value = unitComponent as T;
                return true;
            }
            if (CurrentUnit.TryGetModule(out T module))
            {
                value = module;
                return true;
            }
            
            value = null;
            return false;
        }
        public bool TryGet(Type t, out ModuleBase value)
        {
            if (_modules.TryGetValue(t, out var unitComponent))
            {
                value = unitComponent;
                return true;
            }
            if (CurrentUnit.TryGetModule(t, out var module))
            {
                value = module;
                return true;
            }
            
            value = null;
            return false;
        }
        
        public bool HasOther<T>(T value) where T : class
        {
            foreach (var component in _modules.Values)
                if (component is T) return true;
            return false;
        }
        public bool HasOther<T>() where T : class
        {
            foreach (var component in _modules.Values)
                if (component is T) return true;
            return false;
        }
        public T GetOther<T>() where T : class
        {
            foreach (var component in _modules.Values)
                if (component is T ret) return ret;
            return null;
        }
        public bool GetOther<T>(out T value) where T : class
        {
            foreach (var component in _modules.Values)
                if (component is T ret)
                {
                    value = (T)ret;
                    return true;
                }
            value = null;
            return false;
        }
        
        public virtual void Reset()
        {
            foreach (var component in _modules.Values)
                component.Dispose();
            
            _modules.Clear();

            OnReset();
            Initialize(CurrentUnit);
        }
        private protected virtual void OnReset() { }
        
        public virtual void ResetModules()
        {
            foreach (var component in _modules.Values)
            {
                component.Reset();
                OnResetModule(component);
            }
            foreach (var component in _modules.Values)
            {
                component.Start();
            }
        }
        private protected virtual void OnResetModule(TModule component) { }
        
        public virtual bool IsUpdating()
            => CurrentUnit.SwitchState && UpdateCondition();
        private protected virtual bool UpdateCondition() => true;
        public bool IsModulesInitialized()
        {
            foreach (var component in _modules.Values)
                if (!component.IsInitialized())
                    return false;
            return true;
        }

        public event Action<TModule> OnModuleAdded;
        protected void InvokeOnModuleAdded(TModule module) => OnModuleAdded?.Invoke(module);
        public event Action<TModule> OnModuleRemoved;
        protected void InvokeOnModuleRemoved(TModule module) => OnModuleRemoved?.Invoke(module);

        public void Dispose()
        {
            foreach (var component in _modules.Values)
                component.Dispose();
        }
        private protected virtual void OnDispose() { }
    }
    
    [Serializable]
    public abstract class UpdatableModuleController<TModule, TTemplate>
        : ModuleController<TModule, TTemplate>, IUpdatableModuleController
        where TModule : ModuleBase, new()
        where TTemplate : ITemplate<TModule>
    {
        private readonly List<IUpdateModule> _updateActions = new ();
        private readonly List<IFixedUpdateModule> _fixedUpdateActions = new ();
        private readonly List<ILateUpdateModule> _lateUpdateActions = new ();
        
        public void Update(float deltaTime)
        {
            if (!IsUpdating()) return;
            OnControllerUpdate(deltaTime);
            for (int i = 0; i < _updateActions.Count; i++) 
                _updateActions[i].OnUpdate(deltaTime);    
        }
        public void FixedUpdate(float deltaTime)
        {
            if (!IsUpdating()) return;
            OnControllerFixedUpdate(deltaTime);
            for (int i = 0; i < _fixedUpdateActions.Count; i++) 
                _fixedUpdateActions[i].OnFixedUpdate(deltaTime);    
        }
        public void LateUpdate(float deltaTime)
        {
            if (!IsUpdating()) return;
            OnControllerLateUpdate(deltaTime);
            for (int i = 0; i < _lateUpdateActions.Count; i++) 
                _lateUpdateActions[i].OnLateUpdate(deltaTime);    
        }
        
        private protected virtual void OnControllerUpdate(float deltaTime) { }
        private protected virtual void OnControllerFixedUpdate(float deltaTime) { }
        private protected virtual void OnControllerLateUpdate(float deltaTime) { }
        
        protected sealed override void AddedModulePostprocess(TModule module)
        {
            if (module is IUpdateModule updateModule)
                _updateActions.Add(updateModule);
            if (module is IFixedUpdateModule fixedUpdateModule)
                _fixedUpdateActions.Add(fixedUpdateModule);
            if (module is ILateUpdateModule lateUpdateModule)
                _lateUpdateActions.Add(lateUpdateModule);
        }
        protected override void RemovedModulePostprocess(TModule module)
        {                    
            if (module is IUpdateModule updateModule
                && _updateActions.Contains(updateModule))
                _updateActions.FastRemove(updateModule);
            if (module is IFixedUpdateModule fixedUpdateModule
                && _fixedUpdateActions.Contains(fixedUpdateModule))
                _fixedUpdateActions.FastRemove(fixedUpdateModule);
            if (module is ILateUpdateModule lateUpdateModule
                && _lateUpdateActions.Contains(lateUpdateModule))
                _lateUpdateActions.FastRemove(lateUpdateModule);
        }
        
        private protected sealed override void OnReset()
        {
            _updateActions.Clear();
            _fixedUpdateActions.Clear();
            _lateUpdateActions.Clear();
            OnResetUpdatable();
        }
        private protected virtual void OnResetUpdatable() { }
    }
}