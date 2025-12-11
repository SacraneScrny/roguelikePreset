using System;
using System.Collections;
using System.Collections.Generic;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Extensions;
using Sackrany.Unit.Abstracts;
using Sackrany.Unit.Base;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

namespace Sackrany.Unit.ModuleSystem.Main
{
    public abstract class SimpleModuleController<TModule, TTemplate>
        : IModuleConnector<TModule>, ISimpleModuleController
        where TModule : ModuleBase, new()
        where TTemplate : ITemplate<TModule>
    {
        public TTemplate[] Default;
        
        private protected readonly Dictionary<Type, TModule> _modules = new ();
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
            instance.Awake(CurrentUnit);
            if (!instance.DependencyCheck()) return false; 
            instance.Start();
            AddedModulePostprocess(instance);
            OnModuleAdded?.Invoke(instance);
            return true;
        }
        private protected virtual bool Add_AlreadyExist<T>(T template) where T : TTemplate => false;
        
        public bool Add<T>(T[] templates) where T : TTemplate
        {
            bool allAdded = true;
            for (int i = 0; i < templates.Length; i++)
                allAdded &= Add(templates[i]);
            return allAdded;
        }
        bool IModuleConnector<TModule>.Add<T>(T[] templates)
        {
            bool allAdded = true;
            for (int i = 0; i < templates.Length; i++)
                allAdded &= AddInternal((TTemplate)(object)templates[i]);
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
        public TModule Get<T>() where T : TModule
        {
            return (T)_modules[typeof (T)];
        }
        public TModule Get<K>(K template) where K : ITemplate<TModule>
        {
            return _modules[template.GetModuleType()];
        }
        public bool TryGet<T>(out T value) where T : TModule
        {
            if (_modules.TryGetValue(typeof(T), out var unitComponent))
            {
                value = (T)unitComponent;
                return true;
            }
            value = default(T);
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
        }
        private protected virtual void OnReset() { }
        
        public virtual void ResetModules()
        {
            foreach (var component in _modules.Values)
            {
                component.Reset();
                OnResetModule(component);
            }
        }
        private protected virtual void OnResetModule(TModule component) { }
        
        public virtual bool IsUpdating()
            => CurrentUnit.SwitchState;
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

    public abstract class ModuleController<TModule, TTemplate>
        : SimpleModuleController<TModule, TTemplate>, IModuleController
        where TModule : ModuleBase, new()
        where TTemplate : ITemplate<TModule>
    {
        public readonly ExpandedBool IsWorking = new (true);
        
        private readonly Queue<TTemplate> _addQueue = new ();
        private readonly Queue<Type> _removeQueue = new ();
        
        private Coroutine _addQueueCoroutine;
        private Coroutine _removeQueueCoroutine;
        
        protected override void OnInitialize(UnitBase unit)
        {
            _addQueueCoroutine = CurrentUnit.StartCoroutine(ModulesAddQueue());
            _removeQueueCoroutine = CurrentUnit.StartCoroutine(ModulesRemoveQueue());
            CurrentUnit.OnUnitSwitched += OnUnitSwitched;
        }
        private void OnUnitSwitched(UnitBase unit, bool value)
        {
            if (!value) return;
            
            if (_addQueueCoroutine != null)
                CurrentUnit.StopCoroutine(_addQueueCoroutine);
            if (_removeQueueCoroutine != null)
                CurrentUnit.StopCoroutine(_removeQueueCoroutine);
            _addQueueCoroutine = CurrentUnit.StartCoroutine(ModulesAddQueue());
            _removeQueueCoroutine = CurrentUnit.StartCoroutine(ModulesRemoveQueue());
        }
        
        private IEnumerator ModulesAddQueue()
        {
            while (true)
            {
                yield return new WaitWhile(() => _addQueue.Count == 0);
                List<TModule> modules = new List<TModule>();
                
                while (_addQueue.Count > 0)
                {
                    var template = _addQueue.Dequeue();
                    if (_modules.ContainsKey(template.GetModuleType())) continue;
                    
                    var module = template.GetModuleInstance();

                    if (module is Module<TModule> moduleInstance)
                    {
                        moduleInstance.SetConnector(this);
                    }

                    module.Awake(CurrentUnit);
                    modules.Add(module);
                    _modules.Add(module.GetType(), module);
                }
                
                if (modules.Count == 0)
                    continue;

                yield return new WaitForEndOfFrame();

                for (int i = 0; i < modules.Count; i++)
                {
                    if (!modules[i].DependencyCheck())
                    {
                        _modules.Remove(modules[i].GetType());
                        modules[i].Dispose();
                        continue;
                    }
                    
                    modules[i].Start();
                    AddedModulePostprocess(modules[i]);
                    InvokeOnModuleAdded(modules[i]);
                }
            }
        }
        private IEnumerator ModulesRemoveQueue()
        {
            while (true)
            {
                yield return new WaitWhile(() => _removeQueue.Count == 0);

                while (_removeQueue.Count > 0)
                {
                    var moduleType = _removeQueue.Dequeue();
                    var module = _modules[moduleType];
                    module.Dispose();
                    
                    _modules.Remove(moduleType);
                    InvokeOnModuleRemoved(module);
                    RemovedModulePostprocess(module);
                }
            }
        }

        private protected override bool AddInternal(TTemplate template)
        {
            if (_modules.ContainsKey(template.GetModuleType()))
            {
                return Add_AlreadyExist(template);
            }
            _addQueue.Enqueue(template);
            return true;
        }

        public sealed override bool Remove<T>()
        {
            if (!_modules.ContainsKey(typeof(T))) return false;
            if (_removeQueue.Contains(typeof(T))) return false;
            _removeQueue.Enqueue(typeof(T));
            return true;
        }
        public sealed override bool Remove(params Type[] types)
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
                if (_removeQueue.Contains(types[i]))
                {
                    allRemoved = false;
                    continue;
                }
                _removeQueue.Enqueue(types[i]);
            }
            return allRemoved;
        }
        
        public sealed override void Reset()
        {
            if (_addQueueCoroutine != null)
                CurrentUnit.StopCoroutine(_addQueueCoroutine);
            if (_removeQueueCoroutine != null)
                CurrentUnit.StopCoroutine(_removeQueueCoroutine);
            
            foreach (var component in _modules.Values)
                component.Dispose();
            
            IsWorking.Clear();
            
            _modules.Clear();
            _addQueue.Clear();
            _addQueue.EnqueueAll(Default);
            _removeQueue.Clear();

            _addQueueCoroutine = CurrentUnit.StartCoroutine(ModulesAddQueue());
            _removeQueueCoroutine = CurrentUnit.StartCoroutine(ModulesRemoveQueue());
            OnReset();
        }
        public sealed override bool IsUpdating() => base.IsUpdating() && IsWorking;
        public sealed override void ResetModules()
        {
            foreach (var component in _modules.Values)
            {
                component.Reset();
                OnResetModule(component);
            }
        }
    }
    
    public abstract class SimpleUpdatableModuleController<TModule, TTemplate> 
        : SimpleModuleController<TModule, TTemplate>, IUpdatableModuleController   
        where TModule : ModuleBase, new()
        where TTemplate : ITemplate<TModule>
    {
        private readonly List<IUpdateModule> _updateActions = new ();
        private readonly List<IFixedUpdateModule> _fixedUpdateActions = new ();
        private readonly List<ILateUpdateModule> _lateUpdateActions = new ();
        
        public void Update(float deltaTime)
        {
            if (!IsUpdating()) return;
            for (int i = 0; i < _updateActions.Count; i++) 
                _updateActions[i].OnUpdate(deltaTime);    
        }
        public void FixedUpdate(float deltaTime)
        {
            if (!IsUpdating()) return;
            for (int i = 0; i < _fixedUpdateActions.Count; i++) 
                _fixedUpdateActions[i].OnFixedUpdate(deltaTime);    
        }
        public void LateUpdate(float deltaTime)
        {
            if (!IsUpdating()) return;
            for (int i = 0; i < _lateUpdateActions.Count; i++) 
                _lateUpdateActions[i].OnLateUpdate(deltaTime);    
        }
        
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
        
        private protected override void OnReset()
        {
            _updateActions.Clear();
            _fixedUpdateActions.Clear();
            _lateUpdateActions.Clear();
        }
    }
    
    public abstract class UpdatableModuleController<TModule, TTemplate> 
        : ModuleController<TModule, TTemplate>, ISimpleUpdatableModuleController   
        where TModule : ModuleBase, new()
        where TTemplate : ITemplate<TModule>
    {
        private readonly List<IUpdateModule> _updateActions = new ();
        private readonly List<IFixedUpdateModule> _fixedUpdateActions = new ();
        private readonly List<ILateUpdateModule> _lateUpdateActions = new ();
        
        public void Update(float deltaTime)
        {
            if (!IsUpdating()) return;
            for (int i = 0; i < _updateActions.Count; i++) 
                _updateActions[i].OnUpdate(deltaTime);    
        }
        public void FixedUpdate(float deltaTime)
        {
            if (!IsUpdating()) return;
            for (int i = 0; i < _fixedUpdateActions.Count; i++) 
                _fixedUpdateActions[i].OnFixedUpdate(deltaTime);    
        }
        public void LateUpdate(float deltaTime)
        {
            if (!IsUpdating()) return;
            for (int i = 0; i < _lateUpdateActions.Count; i++) 
                _lateUpdateActions[i].OnLateUpdate(deltaTime);    
        }
        
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
        
        private protected override void OnReset()
        {
            _updateActions.Clear();
            _fixedUpdateActions.Clear();
            _lateUpdateActions.Clear();
        }
    }
}