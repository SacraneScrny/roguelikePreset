using System;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Unit.ModuleSystem.Main;

using UnityEngine;

namespace Sackrany.Unit.Features.ComponentsFeature
{
    [Serializable]
    public sealed class UnitComponentsController : UpdatableModuleController<UnitComponent, IUnitComponentTemplate>
    {
        public readonly ExpandedVector3 UnitScale = Vector3.one;
        private protected override void OnControllerUpdate(float deltaTime)
        {
            CurrentUnit.transform.localScale = UnitScale;
        }
        private protected override void OnResetUpdatable()
        {
            UnitScale.Clear();
        }
    }
    
    public class UnitComponent : LinkedModule<UnitComponent, UnitComponentsController>
    {
        
    }
    
    public interface IUnitComponentTemplate : ITemplate<UnitComponent>
    {
        
    }
}