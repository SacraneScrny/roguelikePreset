using System;
using System.Collections.Generic;

using Sackrany.Unit.Abstracts;
using Sackrany.Unit.ModuleSystem.Main;

using UnityEngine;

namespace Sackrany.Unit.Base
{
    public class SimpleGameUnit : UnitBase
    {
        [SerializeField] private ISimpleModuleController[] Controllers;
        
        private protected override IEnumerable<IBaseModuleController> GetControllers() => Controllers;
        
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
    }
}