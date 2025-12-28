using System;
using System.Collections.Generic;

using Sackrany.CustomRandom.Global;
using Sackrany.ExpandedVariable.Abstracts;
using Sackrany.ExpandedVariable.Entities;
using Sackrany.Unit.ModuleSystem.Updates;

using Unity.Mathematics;

using UnityEngine;

namespace Sackrany.Unit.Features.CameraFeatures.Modules
{
    public class CameraShakeModule : CameraModule, IUpdateModule
    {
        private BaseExpandedVariable<Quaternion>.expandedDelegate _rotationDelegate;
        private BaseExpandedVariable<Vector3>.expandedDelegate _positionDelegate;
        private BaseExpandedVariable<float>.expandedDelegate _fovDelegate;
        
        private readonly List<ShakeEntity> _cameraRotationShakeQueue = new ();
        private readonly List<ShakeEntity> _cameraPositionShakeQueue = new ();
        private readonly List<ShakeEntity> _cameraFovShakeQueue = new ();

        private Quaternion _passiveRotationShake;
        private Vector3 _passivePositionShake;
        private float _passiveFovShake;
        public readonly ExpandedFloat PassiveRotationShake;
        public readonly ExpandedFloat PassivePositionShake;
        public readonly ExpandedFloat PassiveFovShake;

        public CameraShakeModule()
        {
            PassiveRotationShake = 0;
            PassivePositionShake = 0;
            PassiveFovShake = 0;
        }
        protected override void OnStart()
        {
            _rotationDelegate = Controller.CameraRotation.Add_BaseAdditional(() => _shakeRotOffset * _passiveRotationShake);
            _positionDelegate = Controller.CameraPosition.Add_BaseAdditional(() => _shakePosOffset + _passivePositionShake);
            _fovDelegate = Controller.CameraFov.Add_BaseAdditional(() => _shakeFovOffset + _passiveFovShake);
        }
        protected override void OnReset()
        {
            PassiveRotationShake.Clear();
            PassivePositionShake.Clear();
            PassiveFovShake.Clear();
            Controller.CameraRotation.Remove_BaseAdditional(_rotationDelegate);
            Controller.CameraPosition.Remove_BaseAdditional(_positionDelegate);
            Controller.CameraFov.Remove_BaseAdditional(_fovDelegate);
        }

        private Quaternion _shakeRotOffset;
        private Vector3 _shakePosOffset;
        private float _shakeFovOffset;
        public void OnUpdate(float deltaTime)
        {
            UpdatePassiveShake(deltaTime);
            UpdateShakeRotLerp(deltaTime);
            UpdateShakePosLerp(deltaTime);
            UpdateShakeFovLerp(deltaTime);
            
            for (int i = 0; i < _cameraRotationShakeQueue.Count; i++)
            {
                if (_cameraRotationShakeQueue[i].delay > 0)
                {
                    _cameraRotationShakeQueue[i].delay -= deltaTime;
                    continue;
                }
                _cameraRotationShakeQueue[i].duration -= deltaTime;
            }
            for (int i = _cameraRotationShakeQueue.Count - 1; i >= 0; i--)
                if (_cameraRotationShakeQueue[i].duration <= 0)
                    _cameraRotationShakeQueue.RemoveAt(i);
            
            
            for (int i = 0; i < _cameraPositionShakeQueue.Count; i++)
            {
                if (_cameraPositionShakeQueue[i].delay > 0)
                {
                    _cameraPositionShakeQueue[i].delay -= deltaTime;
                    continue;
                }
                _cameraPositionShakeQueue[i].duration -= deltaTime;
            }
            for (int i = _cameraPositionShakeQueue.Count - 1; i >= 0; i--)
                if (_cameraPositionShakeQueue[i].duration <= 0)
                    _cameraPositionShakeQueue.RemoveAt(i);
            
            for (int i = 0; i < _cameraFovShakeQueue.Count; i++)
            {
                if (_cameraFovShakeQueue[i].delay > 0)
                {
                    _cameraFovShakeQueue[i].delay -= deltaTime;
                    continue;
                }
                _cameraFovShakeQueue[i].duration -= deltaTime;
            }
            for (int i = _cameraFovShakeQueue.Count - 1; i >= 0; i--)
                if (_cameraFovShakeQueue[i].duration <= 0)
                    _cameraFovShakeQueue.RemoveAt(i);
        }
        private void UpdateShakeRotLerp(float deltaTime)
        {
            for (int i = 0; i < _cameraRotationShakeQueue.Count; i++)
            {
                if (_cameraRotationShakeQueue[i].delay > 0) continue;
                _shakeRotOffset = Quaternion.Lerp(
                    _shakeRotOffset,
                    Quaternion.Euler(_cameraRotationShakeQueue[i].GetDirOffset()),
                    15f * deltaTime);
            }
            if (_cameraRotationShakeQueue.Count == 0) _shakeRotOffset = Quaternion.Lerp(_shakeRotOffset, Quaternion.identity, 15f * deltaTime);
        }
        private void UpdateShakePosLerp(float deltaTime)
        {
            for (int i = 0; i < _cameraPositionShakeQueue.Count; i++)
            {
                if (_cameraPositionShakeQueue[i].delay > 0) continue;
                _shakePosOffset = Vector3.Lerp(
                    _shakePosOffset,
                    _cameraPositionShakeQueue[i].GetDirOffset(),
                    15f * deltaTime);
            }
            if (_cameraPositionShakeQueue.Count == 0) _shakePosOffset = Vector3.Lerp(_shakePosOffset, Vector3.zero, 15f * deltaTime);
        }
        private void UpdateShakeFovLerp(float deltaTime)
        {
            for (int i = 0; i < _cameraFovShakeQueue.Count; i++)
            {
                if (_cameraFovShakeQueue[i].delay > 0) continue;
                _shakeFovOffset = Mathf.Lerp(
                    _shakeFovOffset,
                    _cameraFovShakeQueue[i].GetDirOffset().x,
                    15f * deltaTime);
            }
            if (_cameraFovShakeQueue.Count == 0) _shakeFovOffset = Mathf.Lerp(_shakeFovOffset, 0, 15f * deltaTime);
        }
        
        private float passiveShakeTimer;
        private void UpdatePassiveShake(float deltaTime)
        {
            passiveShakeTimer += deltaTime * 40f;
            if (passiveShakeTimer >= 1000f) passiveShakeTimer = 0;

            float nx = noise.cnoise(new float2(passiveShakeTimer, 0f));
            float ny = noise.cnoise(new float2(0f, passiveShakeTimer));
            float nz = noise.cnoise(new float2(passiveShakeTimer, passiveShakeTimer));
            
            // === ROTATION ===
            Vector3 rotDir = new Vector3(nx, ny, nz);
            if (rotDir.sqrMagnitude > 0.0001f)
                rotDir.Normalize();

            Quaternion rotTarget =
                Quaternion.AngleAxis(PassiveRotationShake, rotDir);

            _passiveRotationShake =
                Quaternion.Slerp(_passiveRotationShake, rotTarget, deltaTime * 5f);

            // === POSITION ===
            Vector3 posTarget = new Vector3(nx, ny, nz) * PassivePositionShake;

            _passivePositionShake =
                Vector3.Lerp(_passivePositionShake, posTarget, deltaTime * 5f);

            // === FOV ===
            float fovNoise = noise.cnoise(new float2(passiveShakeTimer, 42.42f));
            float fovTarget = fovNoise * PassiveFovShake;

            _passiveFovShake =
                Mathf.Lerp(_passiveFovShake, fovTarget, deltaTime * 5f);
        }
        
        public void RotationShake(float duration, float strength, int count = 1)
        {
            RotationShake(Vector3.zero, duration, strength, count);
        }
        public void RotationShake(Vector3 offset, float duration, float strength, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                float delay = 0;
                Vector3 dir = offset + new Vector3
                (GlobalRandom.Current.NextFloat(-90, 90), 
                    GlobalRandom.Current.NextFloat(-90, 90), 
                    GlobalRandom.Current.NextFloat(-90, 90)) * strength;
                _cameraRotationShakeQueue.Add(new ShakeEntity()
                {
                    duration = duration,
                    maxDuration = duration,
                    dir = dir,
                    delay = delay
                });
                delay += duration; 
            }
        }
        
        public void PositionShake(float duration, float strength, int count = 1)
        {
            PositionShake(Vector3.zero, duration, strength, count);
        }
        public void PositionShake(Vector3 offset, float duration, float strength, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                float delay = 0;
                Vector3 dir = (offset + new Vector3
                (GlobalRandom.Current.NextFloat(-1, 1), 
                    GlobalRandom.Current.NextFloat(-1, 1), 
                    GlobalRandom.Current.NextFloat(-1, 1))) * strength;
                _cameraPositionShakeQueue.Add(new ShakeEntity()
                {
                    duration = duration,
                    maxDuration = duration,
                    dir = dir,
                    delay = delay
                });
                delay += duration; 
            }
        }
        
        public void FovShake(float duration, float strength, int count = 1)
        {
            FovShake(0, duration, strength, count);
        }
        public void FovShake(float offset, float duration, float strength, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                float delay = 0;
                Vector3 dir = new Vector3(offset + GlobalRandom.Current.NextFloat(-1, 1), 0, 0) * strength;
                _cameraFovShakeQueue.Add(new ShakeEntity()
                {
                    duration = duration,
                    maxDuration = duration,
                    dir = dir,
                    delay = delay
                });
                delay += duration; 
            }
        }
        
        public class ShakeEntity
        {
            public Vector3 dir;
            public float duration;
            public float maxDuration;
            public float delay;
            
            public Vector3 GetDirOffset() => dir * GetStrength();
            public float GetStrength() => (duration / maxDuration);
        }
    }
    
    [Serializable]
    public struct CameraShake : CameraModuleTemplate
    {
        public Type GetModuleType() => typeof(CameraShakeModule);
        public CameraModule GetModuleInstance() => new CameraShakeModule();
    }
}