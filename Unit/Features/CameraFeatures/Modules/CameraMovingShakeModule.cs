using System;

using Sackrany.ExpandedVariable.Abstracts;
using Sackrany.Unit.Features.ComponentsFeature;
using Sackrany.Unit.Features.ComponentsFeature.Modules;
using Sackrany.Unit.ModuleSystem.Updates;

using Unity.Mathematics;

using UnityEngine;

using Random = Unity.Mathematics.Random;

namespace Sackrany.Unit.Features.CameraFeatures.Modules
{
    public class CameraMovingShakeModule : CameraModule, IUpdateModule
    {
        private readonly float _noiseScale;
        private readonly float _strength;
        private readonly float _verticalShakeStrength;
        private readonly float _deltaSensetivity;
        private readonly Vector2 _clamps;
        private BaseExpandedVariable<Quaternion>.expandedDelegate _shakeDelegate;
        
        protected override void OnStart()
        {
            _shakeDelegate = Controller.CameraRotation.Add_BaseAdditional(Shake);
        }
        protected override void OnReset()
        {
            Controller.CameraRotation.Remove_BaseAdditional(_shakeDelegate);
        }
        private Quaternion Shake()
        {
            return GetShakeQuaternion();
        }
        
        public CameraMovingShakeModule(float noiseScale, float strength, float verticalShakeStrength, float deltaSensetivity, Vector2 clams)
        {
            _noiseScale = noiseScale * 1.9f;
            _strength = strength * 190.6f;
            _verticalShakeStrength = verticalShakeStrength / 10f;
            _deltaSensetivity = deltaSensetivity;
            _clamps = new Vector2(clams.x * 0.06f, clams.y * 0.12f);
        }
        
        private float _time;
        private float _currentDelta;
        public void OnUpdate(float deltaTime)
        {
            var delta = Unit.Conditions.GetDeltaPosition;
            var deltaM = delta.magnitude;
            _currentDelta = Mathf.Lerp(_currentDelta, deltaM, 8f * deltaTime);
            _time += deltaTime * _noiseScale * math.tanh(_currentDelta * _deltaSensetivity) / 10f;
            _time = _time >= 1f ? 0f : _time;

            jumpVelocity = Mathf.Lerp(jumpVelocity, math.min(delta.y * _verticalShakeStrength, _clamps.y), 8f * deltaTime);
        }
        
        private float jumpVelocity = 0;
        private float offsetX = 45346;
        private float offsetY = 21886;
        public Quaternion GetShakeQuaternion()
        {
            offsetX += Time.deltaTime / 100f;
            offsetY += Time.deltaTime / 100f;
            
            float absJump = math.abs(jumpVelocity);
            
            float x = math.sin(_time * offsetX) + jumpVelocity * 10;
            float y = math.cos(_time + offsetY) / (absJump * 10 + 1);
            float z = (math.sin(_time * offsetY) + math.cos(_time + offsetX)) / (absJump * 10 + 1);

            float pwr = _currentDelta + absJump;
            pwr = math.min(math.tanh(pwr * _deltaSensetivity), _clamps.x);
            float3 euler = new float3(x, y, z) * _strength * pwr;

            return Quaternion.Euler(euler);
        }
    }

    [Serializable]
    public struct CameraMovingShake : CameraModuleTemplate
    {
        public float NoiseScale;
        public float Strength;
        public float VerticalShakeStrength;
        public float DeltaSensetivity;
        public Vector2 Clamps;
        
        public Type GetModuleType() => typeof(CameraMovingShakeModule);
        public CameraModule GetModuleInstance() => new CameraMovingShakeModule(NoiseScale, Strength, VerticalShakeStrength, DeltaSensetivity, Clamps);
    }
}