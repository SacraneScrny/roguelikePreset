using System;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Unit.ModuleSystem.Main;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

namespace Sackrany.Unit.Features.AIFeature.Modules
{
    public class LookAtPlayerModule : AIModule, IUpdateModule
    {
        private LookAtPlayer _template;

        public ExpandedFloat Speed;
        public LookAtPlayerModule Construct(LookAtPlayer template)
        {
            _template = template;
            Speed = _template.speed;
            return this;
        }
        protected override void OnReset()
        {
            Speed.Clear();
        }
        
        public void OnUpdate(float deltaTime)
        {
            var self = Unit.transform;
            var targetPos = PlayerUnit.transform.position;
            var selfPos = self.position;

            if (_template.onlyY)
            {
                targetPos.y = selfPos.y;
            }

            var dir = targetPos - selfPos;
            if (dir.sqrMagnitude < 0.0001f)
                return;

            var targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

            self.rotation = Quaternion.RotateTowards(
                self.rotation,
                targetRot,
                Speed * deltaTime
            );
        }
    }
    
    [Serializable]
    public struct LookAtPlayer : AIModuleTemplate
    {
        public float speed;
        public bool onlyY;
        public Type GetModuleType() => typeof(LookAtPlayerModule);
        public AIModule GetModuleInstance() => new LookAtPlayerModule().Construct(this);
    }
}