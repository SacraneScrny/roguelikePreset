using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Extensions;
using Sackrany.Hash;
using Sackrany.Pool.Abstracts;
using Sackrany.Unit.Data;
using Sackrany.Unit.ModuleSystem.Main;

using UnityEngine;

namespace Sackrany.Unit.Abstracts
{
    public abstract class UnitBase : MonoBehaviour, IEquatable<UnitBase>, IPoolable
    {
        public bool AutoSwitch;
        public bool AutoAwake;
        public ExpandedFloat UnitTimeFlow = new (1f);
        public UnitEvent Event = new ();
        public UnitTag Tag = new ();
        public UnitConditions Conditions = new ();
        
        private protected readonly List<IUpdatable> _updateActions = new ();

        private UnitArchetype _archetype;
        private uint _hash;
        private bool _isInitialized;
        private bool _switchState = true;
        private bool _isQuitting;
        
        private void Awake()
        {
            UnitTimeFlow.GetValue();
            _archetype = new UnitArchetype(this);
            Application.quitting += () => _isQuitting = true;
            if (AutoAwake)
                Initialize();
            if (AutoSwitch)
                Switch(false);
        }
        
        private protected void SetUpTimeFlow()
        {
            UnitTimeFlow.Clear();
            UnitTimeFlow.Add_Multiply(UnitManager.Instance.UnitsTimeFlow);
        }
        private protected void SetUpUnitData()
        {
            Event = new UnitEvent();
            Event.Initialize(this);
            Tag.Initialize(this);
            Conditions.Initialize(this);
        }
        
        private void Update()
        {
            if (!_isInitialized) return;
            float dt = Time.deltaTime * UnitTimeFlow;
            Conditions.Update();
            for (var i = 0; i < _updateActions.Count; i++)
                _updateActions[i].Update(dt);
        }
        private void FixedUpdate()
        {
            if (!_isInitialized) return;
            float dt = Time.fixedDeltaTime * UnitTimeFlow;
            Conditions.FixedUpdate();
            for (var i = 0; i < _updateActions.Count; i++)
                _updateActions[i].FixedUpdate(dt);
        }
        private void LateUpdate()
        {
            if (!_isInitialized) return;
            float dt = Time.deltaTime * UnitTimeFlow;
            for (var i = 0; i < _updateActions.Count; i++)
                _updateActions[i].LateUpdate(dt);
        }
        
        public event Action<UnitBase> OnUnitInitialized;
        public event Action<UnitBase> OnUnitReset;
        public event Action<UnitBase, bool> OnUnitSwitched;
        
        public uint GetId() => _hash;
        public UnitArchetype GetArchetype() => _archetype;
        private void OnDestroy()
        {
            if (!_isInitialized) return;
            if (_isQuitting) return;
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
            _isInitialized = true;
            SetUpTimeFlow();
            SetUpUnitData();
            
            OnInitialize();
            foreach (var controller in GetControllers())
            {
                controller.Initialize(this);
                if (controller is IUpdatable updatable)
                    _updateActions.Add(updatable);
            }
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
            Conditions.Reset();
            
            _updateActions.Clear();
            foreach (var controller in GetControllers())
            {
                controller.Reset();
                if (controller is IUpdatable updatable)
                    _updateActions.Add(updatable);
            }
        }
        private protected void ResetUnitControllers()
        {
            foreach (var controller in GetControllers())
                controller.Reset();
        }
        
        public abstract IEnumerable<IBaseModuleController> GetControllers();
        
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

        public bool Equals(UnitBase other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;

            return _hash == other._hash;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((UnitBase)obj);
        }
        public override int GetHashCode()
        {
            return unchecked((int)_hash);
        }

        public static bool operator ==(UnitBase left, UnitBase right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }
        public static bool operator !=(UnitBase left, UnitBase right)
            => !(left == right);
        
        public void OnPooled()
        {
            OnBeforePooled();
            gameObject.SetActive(true);
            UnitManager.RegisterUnit(this);
            Switch(true);
            ResetUnit();
        }
        private protected virtual void OnBeforePooled() { }
        
        public void OnReleased()
        {
            UnitManager.UnregisterUnit(this);
            Switch(false);
            OnAfterReleased();
            gameObject.SetActive(false);
        }
        private protected virtual void OnAfterReleased() { }
    }
}