using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Sackrany.Unit.ModuleSystem.Main;

using UnityEngine;

namespace Sackrany.Unit.Modules.EffectsFeature
{
    public sealed class UnitEffectsController : SimpleModuleController<UnitEffectBase, IUnitEffectTemplate>
    {        
        private readonly Dictionary<Type, List<Coroutine>> _effectCoroutines = new();
        
        private protected override bool Add_AlreadyExist<T>(T template)
        {
            Get(template).IncreaseAmount(1);
            return true;
        }
        
        public Coroutine StartEffectCoroutine(UnitEffectBase unitEffect, IEnumerator coroutine)
        {
            ClearNullCoroutines();
            var c = CurrentUnit.StartCoroutine(coroutine);
            
            if (!_effectCoroutines.TryGetValue(unitEffect.GetType(), out var coroutines))
            {
                coroutines = new List<Coroutine>();
                _effectCoroutines.Add(unitEffect.GetType(), coroutines);
            }
            
            coroutines.Add(c);
            return c;
        }
        public bool StopEffectCoroutine(UnitEffectBase unitEffect, Coroutine coroutine)
        {
            ClearNullCoroutines();
            
            if (!_effectCoroutines.TryGetValue(unitEffect.GetType(), out var coroutines))
                return false;
            if (coroutine == null) return false;
            
            CurrentUnit.StopCoroutine(coroutine);
            coroutines.Remove(coroutine);
            return true;
        }
        public void StopAllEffectCoroutines(UnitEffectBase unitEffect)
        {
            if (_effectCoroutines.TryGetValue(unitEffect.GetType(), out var coroutines))
                foreach (var coroutine in coroutines.Where(x => x != null))
                    CurrentUnit.StopCoroutine(coroutine);
            _effectCoroutines.Remove(unitEffect.GetType());
            ClearNullCoroutines();
        }
        private void StopAllEffectCoroutines()
        {
            foreach (var c in _effectCoroutines)
            {
                foreach (var coroutine in c.Value.Where(x => x != null))
                    CurrentUnit.StopCoroutine(coroutine);
            }
            _effectCoroutines.Clear();
        }
        private void ClearNullCoroutines()
        {
            foreach (var list in _effectCoroutines)
                list.Value.RemoveAll(x => x == null);
        }
        
        private protected override void OnResetModule(UnitEffectBase component)
        {
            StopAllEffectCoroutines(component);
        }
        private protected override void OnReset()
        {
            StopAllEffectCoroutines();
        }
    }
    
    public class UnitEffectBase : LinkedModule<UnitEffectBase, UnitEffectsController>
    {
        private int _amount;
        public int Amount => _amount;

        //todo cache
        private protected float deltaTime => Time.deltaTime / Unit.UnitTimeFlow;
        private protected float fixedDeltaTime => Time.fixedDeltaTime / Unit.UnitTimeFlow;

        public void IncreaseAmount(int diff)
        {
            _amount += diff;
            if (!IsInitialized()) return;
            OnAmountChanged(diff);
        }
        public void DecreaseAmount(int diff)
        {
            diff = Mathf.Min(_amount, diff);
            _amount -= diff;
            
            if (IsInitialized())
                OnAmountChanged(diff);
            
            if (_amount <= 0)
            {
                if (IsInitialized())
                    OnOutOfAmount();
                Dispose();
            }
        }
        private protected virtual void OnAmountChanged(int offset) { }
        private protected virtual void OnOutOfAmount() { }
        
        protected override void OnReset()
        {
            _amount = 1;
        }

        private protected Coroutine StartCoroutine(IEnumerator routine) => Controller.StartEffectCoroutine(this, routine);
        private protected bool StopCoroutine(Coroutine routine) => Controller.StopEffectCoroutine(this, routine);
        private protected void StopAllCoroutines() => Controller.StopAllEffectCoroutines(this);
    }
    public interface IUnitEffectTemplate : ITemplate<UnitEffectBase> { }
}