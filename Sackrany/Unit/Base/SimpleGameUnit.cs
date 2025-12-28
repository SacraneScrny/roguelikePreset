using System;
using System.Collections.Generic;

using Sackrany.Extensions;
using Sackrany.Unit.Abstracts;
using Sackrany.Unit.ModuleSystem.Main;

using UnityEngine;

namespace Sackrany.Unit.Base
{
    public class SimpleGameUnit : UnitBase
    {
        [SerializeField] private IBaseModuleController[] Controllers;
        
        public override IEnumerable<IBaseModuleController> GetControllers() => Controllers;
        
        public override TController Get<TController>()
        {
            for (int i = 0; i < Controllers.Length; i++)
                if (Controllers[i].GetType() == typeof(TController))
                    return (TController)Controllers[i];
            return default(TController);
        }
        public override bool TryGet<TController>(out TController value)
        {
            for (int i = 0; i < Controllers.Length; i++)
                if (Controllers[i].GetType() == typeof(TController))
                {
                    value = (TController)Controllers[i];
                    return true;
                }
            value = default(TController);
            return false;
        }
        public override bool Add<TController>(TController value)
        {
            for (int i = 0; i < Controllers.Length; i++)
                if (Controllers[i].GetType() == typeof(TController))
                    return false;
            
            Array.Resize(ref Controllers, Controllers.Length + 1);
            Controllers[^1] = value;
            IntegrateController(value);
            
            return true;
        }
        public override bool Remove<TController>(TController value)
        {
            bool has = false;
            for (int i = 0; i < Controllers.Length; i++)
                if (Controllers[i].GetType() == typeof(TController))
                {
                    has = true;
                    break;
                }
            if (!has) return false;
            
            DisposeController(value);
            Controllers.FastRemove(value);
            return true;
        }
    }
}