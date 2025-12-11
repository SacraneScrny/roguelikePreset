using System;
using System.Collections.Generic;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Extensions;
using Sackrany.Unit.Data;
using Sackrany.Unit.ModuleSystem.Main;

using UnityEngine;

namespace Sackrany.Unit.Abstracts
{
    public abstract class UnitBase : MonoBehaviour
    {        
        public ExpandedFloat UnitTimeFlow = new (1f);
        public UnitEvent Event = new ();
        public UnitTag Tag = new ();
        
        private protected readonly List<IUpdatable> _updateActions = new ();
        
        private long _hash;
        private bool _isInitialized;
        private bool _switchState = true;
        
        private protected void SetUpTimeFlow()
        {
            UnitTimeFlow = new ExpandedFloat(1f);
            UnitTimeFlow.Add_Multiply(UnitManager.Instance.UnitsTimeFlow);
        }
        private protected void SetUpUnitData()
        {
            Event = new UnitEvent();
            Event.Initialize(this);
        }
        
        private void Update()
        {
            if (!_isInitialized) return;
            for (var i = 0; i < _updateActions.Count; i++)
                _updateActions[i].Update(Time.deltaTime / UnitTimeFlow);
        }
        private void FixedUpdate()
        {
            if (!_isInitialized) return;
            for (var i = 0; i < _updateActions.Count; i++)
                _updateActions[i].FixedUpdate(Time.deltaTime / UnitTimeFlow);
        }
        private void LateUpdate()
        {
            if (!_isInitialized) return;
            for (var i = 0; i < _updateActions.Count; i++)
                _updateActions[i].LateUpdate(Time.deltaTime / UnitTimeFlow);
        }
        
        private Action<float> _updateAction;
        private Action<float> _fixedUpdateAction;
        private Action<float> _lateUpdateAction;
        
        public event System.Action<UnitBase> OnUnitInitialized;
        public event System.Action<UnitBase> OnUnitReset;
        public event System.Action<UnitBase, bool> OnUnitSwitched;        
        
        public long GetId() => _hash;
        private void OnDestroy()
        {
            if (!_isInitialized) return;
            UnitManager.UnregisterUnit(this);
        }
        
        public bool SwitchState => _switchState;
        public bool IsInitialized
            => _isInitialized;
        public bool IsCompleteInitialized
        {
            get
            {
                if (!_isInitialized) return false;
                return IsControllersInitialized();
            }
        }

        private bool IsControllersInitialized()
        {
            foreach (var controller in GetControllers())
                if (!controller.IsModulesInitialized()) return false;
            return true;
        }
        
        private protected void InitializeUnit()
        {
            if (_isInitialized) return;
            _hash = SimpleId.Next();
            SetUpTimeFlow();
            SetUpUnitData();
            
            OnInitialize();
            foreach (var controller in GetControllers())
            {
                controller.Initialize(this);
                if (controller is IUpdatable updatable)
                    _updateActions.Add(updatable);
            }
            _isInitialized = true;
            UnitManager.RegisterUnit(this);  
            OnUnitInitialized?.Invoke(this);
        }
        private protected virtual void OnInitialize() { }
        
        private protected void SwitchUnit(bool value)
        {
            if (_switchState == value) return;
            _switchState = value;
            OnUnitSwitched?.Invoke(this, value);
        }
        private protected void ResetFullUnit()
        {
            if (!_isInitialized) return;
            OnUnitReset?.Invoke(this);
            
            UnitTimeFlow.Clear();
            Event.Reset();
            Tag.Reset();
        }
        private protected void ResetUnitControllers()
        {
            foreach (var controller in GetControllers())
                controller.Reset();
        }
        
        private protected abstract IEnumerable<IBaseModuleController> GetControllers();
        
        public UnitBase Initialize()
        {
            InitializeUnit();
            return this;
        }
        public UnitBase Switch(bool value)
        {
            SwitchUnit(value);
            return this;
        }
        public UnitBase ResetUnit()
        {
            ResetFullUnit();
            return this;
        }
        public UnitBase ResetControllers()
        {
            ResetUnitControllers();
            return this;
        }

        public abstract TController Get<TController>() where TController : IBaseModuleController;
        public abstract bool TryGet<TController>(out TController value) where TController : IBaseModuleController;
        
        public virtual bool Add<TController>(TController value) where TController : IBaseModuleController => false;
        public virtual bool Remove<TController>(TController value) where TController : IBaseModuleController => false;
        
        private protected void IntegrateController<TController>(TController value) where TController : IBaseModuleController
        {
            value.Initialize(this);
            if (value is IUpdatable updatable)
                _updateActions.Add(updatable);
        }
        private protected void DisposeController<TController>(TController value)
            where TController : IBaseModuleController
        {
            if (value is IUpdatable updatable
                && _updateActions.Contains(updatable))
                _updateActions.FastRemove(updatable);
            value.Dispose();
        }
    }
}