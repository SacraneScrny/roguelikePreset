using System;

using Sackrany.Unit.Features.ComponentsFeature;
using Sackrany.Unit.ModuleSystem.Updates;

using Unity.Mathematics;

using UnityEngine;

namespace Sackrany.Unit.Features.CameraFeatures.Modules
{
    public class CameraMovingFovModule : CameraModule, ILateUpdateModule
    {
        private readonly float _deltaSensetivity;
        private readonly float _clamp;
        private readonly float _strength;
        private readonly float _lerpSpeed;
        
        public CameraMovingFovModule(float deltaSensetivity, float lerpSpeed, float strength, float clamp)
        {
            _deltaSensetivity = deltaSensetivity;
            _clamp = clamp;
            _strength = strength * 50000;
            _lerpSpeed = lerpSpeed;
        }
        protected override void OnStart()
        {
            Controller.CameraFov.Add_PostAdditional(() =>
            {
                var ret = math.clamp(math.tanh(_currentDelta * _deltaSensetivity) * _strength, -_clamp, _clamp);
                return ret;
            });
        }        
        
        private float _currentDelta;
        public void OnLateUpdate(float deltaTime)
        {
            var delta = Unit.Conditions.GetDeltaPosition;
            var deltaM = delta.magnitude;
            _currentDelta = Mathf.Lerp(_currentDelta, deltaM, _lerpSpeed * deltaTime);
        }
    }
    
    [Serializable]
    public struct CameraMovingFov : CameraModuleTemplate
    {
        public float lerpSpeed;
        public float deltaSensetivity;
        public float strength;
        public float clamp;
        
        public Type GetModuleType() => typeof(CameraMovingFovModule);
        public CameraModule GetModuleInstance() => new CameraMovingFovModule(deltaSensetivity, lerpSpeed, strength, clamp);
    }
}