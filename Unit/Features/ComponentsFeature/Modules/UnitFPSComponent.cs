using System;

using Sackrany.ExpandedVariable.Abstracts;
using Sackrany.Pool.Extensions;
using Sackrany.Unit.Abstracts;
using Sackrany.Unit.ModuleSystem;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

namespace Sackrany.Unit.Features.ComponentsFeature.Modules
{
    public class UnitFPSComponent : UnitComponent, IFixedUpdateModule, IUpdateModule
    {
        [Template] private UnitFPS _template;
        [Dependency] private UnitMovementComponent _movement;
        [Dependency(true)] private UnitJumpingComponent _jumping;
        
        protected override void OnStart()
        {
            _movement.IsSprinting.Add_BaseAdditional(() => Input.GetKey(KeyCode.LeftShift));
        }
        
        public void OnFixedUpdate(float deltaTime)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            var move = Unit.transform.right * x + Unit.transform.forward * z;
            _movement.Move(move);
        }
        public void OnUpdate(float deltaTime)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                _jumping?.Jump();
            if (Input.GetKey(KeyCode.R))
            {
                var b = _template.bullet.POOL();
                b.transform.position = Unit.transform.position;
                b.transform.rotation = Unit.transform.rotation;
            }
        }
    }
    
    [Serializable]
    public struct UnitFPS : IUnitComponentTemplate
    {
        public UnitBase bullet;
        public Type GetModuleType() => typeof(UnitFPSComponent);
        public UnitComponent GetModuleInstance() => new UnitFPSComponent();
    }
}