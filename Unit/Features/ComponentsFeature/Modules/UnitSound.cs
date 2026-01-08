using System;
using System.Collections.Generic;
using System.Linq;

using Sackrany.CMS;
using Sackrany.CustomRandom.Extensions;
using Sackrany.CustomRandom.Global;
using Sackrany.ExpandedVariable.Entities;
using Sackrany.Pool.Extensions;
using Sackrany.Unit.ModuleSystem.Updates;

namespace Sackrany.Unit.Features.ComponentsFeature.Modules
{
    public class UnitSoundComponent : UnitComponent, IUpdateModule
    {
        private readonly HashCMS[] _stepSounds;
        private readonly HashCMS[] _jumpSounds;
        private readonly ExpandedFloat _stepDistance;

        private Queue<HashCMS> _randomSteps = new Queue<HashCMS>();
        private Queue<HashCMS> _randomJumps = new Queue<HashCMS>();

        public UnitSoundComponent(HashCMS[] stepSounds, HashCMS[] jumpSounds, float stepDistance)
        {
            _stepDistance = stepDistance;
            _stepSounds = stepSounds;
            _jumpSounds = jumpSounds;
        }
        protected override void OnStart()
        {
            UpdateRandomLists();
            _stepDistance.Clear();
            if (TryGet(out UnitMovementComponent movement))
            {
                _stepDistance.Add_Multiply(() => 1f / movement.LastMoveSpeed);
                _stepDistance.Add_PostAdditional(() => movement.IsGrounded ? 0 : 100);
                movement.OnGrounded += (_) => distance = float.MaxValue;
            }
            if (TryGet(out UnitJumpingComponent jumping))
            {
                jumping.OnJump += () =>
                {
                    var sound = _randomJumps.Dequeue();
                    var soundObject = CMS_Manager.CMS.Get(sound).POOL();
                    soundObject.transform.position = Unit.transform.position;
                    UpdateRandomLists();
                };
            }
        }

        private void UpdateRandomLists()
        {
            if (_randomSteps.Count == 0) _randomSteps = new Queue<HashCMS>(_stepSounds.Shuffle());
            if (_randomJumps.Count == 0) _randomJumps = new Queue<HashCMS>(_jumpSounds.Shuffle());
        }
        
        private float distance;
        public void OnUpdate(float deltaTime)
        {
            float mgn = Unit.Conditions.GetDeltaPositionMagnitude;
            if (mgn <= float.Epsilon * 2) return;
            distance += deltaTime;
            if (distance > _stepDistance)
            {
                var sound = _randomSteps.Dequeue();
                var soundObject = CMS_Manager.CMS.Get(sound).POOL();
                soundObject.transform.position = Unit.transform.position;
                distance = 0;
                UpdateRandomLists();
            }
        }
    }
    
    [Serializable]
    public struct UnitSound : IUnitComponentTemplate
    {
        public HashCMS[] stepSounds;
        public HashCMS[] jumpSounds;
        public float stepDistance;
        public Type GetModuleType() => typeof(UnitSoundComponent);
        public UnitComponent GetModuleInstance() => new UnitSoundComponent(stepSounds, jumpSounds, stepDistance);
    }
}