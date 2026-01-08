using System;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Unit.Features.AIFeature.Modules;
using Sackrany.Unit.ModuleSystem;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

namespace Sackrany.Unit.Features.ComponentsFeature.Modules
{
    public class UnitGravityComponent : UnitComponent, IUpdateModule
    {
        [Dependency] private CharacterController _characterController;
        
        public readonly ExpandedFloat Gravity;
        
        public UnitGravityComponent(float dGravity)
        {
            Gravity = dGravity;
        }
        protected override void OnStart()
        {
            _characterController = Unit.GetComponent<CharacterController>();
        }
        protected override void OnReset()
        {
            Gravity.Clear();
            Get<UnitMovementComponent>();
        }
        
        public void OnUpdate(float deltaTime)
        {
            if (_characterController.isGrounded) return;
            _characterController.Move(Vector3.up * Gravity * deltaTime);
        }
    }
    
    [Serializable]
    public struct UnitGravity : IUnitComponentTemplate
    {
        public float gravity;
        public Type GetModuleType() => typeof(UnitGravityComponent);
        public UnitComponent GetModuleInstance() => new UnitGravityComponent(gravity);
    }
}