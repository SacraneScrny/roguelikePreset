using System;
using System.Collections.Generic;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Pool.Abstracts;
using Sackrany.Unit.Abstracts;
using Sackrany.Unit.Features.ComponentsFeature;
using Sackrany.Unit.ModuleSystem.Main;

using UnityEngine;

namespace Sackrany.Unit.Base
{
    public class GameUnit : UnitBase, IPoolable
    {
        [SerializeField][SerializeReference][SubclassSelector] private protected List<IBaseModuleController> Controllers = new ();
        private readonly Dictionary<Type, IBaseModuleController> _cachedControllers = new ();
        
        private protected override void OnInitialize()
        {
            CacheControllers();
        }
        private void CacheControllers()
        {
            _cachedControllers.Clear();
            foreach (var controller in Controllers)
            {
                _cachedControllers.Add(controller.GetType(), controller);
            }
        }
        
        public override IEnumerable<IBaseModuleController> GetControllers() => _cachedControllers.Values;
        
        public override TController Get<TController>()
        {
            if (_cachedControllers.TryGetValue(typeof(TController), out var controller))
                return (TController)controller;
            return default(TController);
        }
        public override bool TryGet<TController>(out TController value)
        {
            if (_cachedControllers.TryGetValue(typeof(TController), out var controller))
            {
                value = (TController)controller;
                return true;
            }
            value = default(TController);
            return false;
        }
        public override bool Add<TController>(TController value)
        {
            if (_cachedControllers.ContainsKey(typeof(TController))) return false;
            Controllers.Add(value);
            _cachedControllers.Add(typeof(TController), value);
            IntegrateController(value);
            return true;
        }
        public override bool Remove<TController>(TController value)
        {
            if (!_cachedControllers.ContainsKey(typeof(TController))) return false;
            if (!Controllers.Contains(value)) return false;
            DisposeController(value);
            _cachedControllers.Remove(typeof(TController));
            Controllers.Remove(value);
            return true;
        }
        
        public void OnPooled()
        {
            Switch(true);
            ResetUnit();
        }
        public void OnReleased()
        {
            Switch(false);
        }
    }
}