using System;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Unit.Features.ComponentsFeature.Modules;
using Sackrany.Unit.ModuleSystem;
using Sackrany.Unit.ModuleSystem.Main;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

namespace Sackrany.Unit.Features.AIFeature.Modules
{
    public class AIMoveModule : AIModule, IUpdateModule
    {
        public ExpandedVector3 MoveDirection;
        public ExpandedBool IsMoving;

        [Dependency] private UnitMovementComponent _movement;
        private AIMove _template;
        public AIMoveModule Construct(AIMove template)
        {
            IsMoving = true;
            MoveDirection = template.defaultDirection;
            _template = template;
            return this;
        }
        protected override void OnStart()
        {
            
        }
        protected override void OnReset()
        {
            MoveDirection.Clear();
        }
        
        public void OnUpdate(float deltaTime)
        {
            if (!IsMoving.GetValue()) return;
            _movement.MoveRelative(MoveDirection);
        }
    }
    
    [Serializable]
    public struct AIMove : AIModuleTemplate
    {
        public Vector3 defaultDirection;
        public Type GetModuleType() => typeof(AIMoveModule);
        public AIModule GetModuleInstance() => new AIMoveModule().Construct(this);
    }
}