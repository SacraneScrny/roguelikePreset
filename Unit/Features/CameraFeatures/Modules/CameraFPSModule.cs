using System;

using Sackrany.CustomRandom.Global;
using Sackrany.ExpandedVariable.Abstracts;
using Sackrany.Unit.Features.ComponentsFeature.Modules;
using Sackrany.Unit.ModuleSystem;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Sackrany.Unit.Features.CameraFeatures.Modules
{
    public class CameraFPSModule : CameraModule, IUpdateModule
    {
        [Dependency(true)] private CameraShakeModule _cameraShakeModule;
        [Dependency(true)] private UnitMovementComponent _unitMovementComponent;
        [Dependency(true)] private UnitHealthComponent _unitHealthComponent;
        
        private CameraFPS _template;
        public CameraFPSModule Construct(CameraFPS template)
        {
            _template = template;
            return this;
        }
        
        protected override void OnStart()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Controller.CameraRotation.Add_BaseAdditional(GetRotation);
            Controller.CameraPosition.Add_BaseAdditional(GetPosition);

            if (_cameraShakeModule != null)
            {
                if (_unitMovementComponent != null)
                {
                    _unitMovementComponent.OnGrounded += (vel) => _cameraShakeModule.RotationShake(
                        new Vector3(9, 0, 0),
                        GlobalRandom.Current.NextFloat(0.25f, 0.45f),
                        vel * 0.1f);
                    _unitMovementComponent.OnGrounded += (vel) => _cameraShakeModule.PositionShake(
                        Vector3.down * 1.5f,
                        GlobalRandom.Current.NextFloat(0.1f, 0.15f),
                        vel * 0.6f);
                    _unitMovementComponent.OnGrounded += (vel) => _cameraShakeModule.FovShake(
                        -2,
                        GlobalRandom.Current.NextFloat(0.1f, 0.15f),
                        vel * 5f);
                }

                if (_unitHealthComponent != null)
                {
                    _unitHealthComponent.OnDamaged += (dmg) => _cameraShakeModule.RotationShake(
                        GlobalRandom.Current.NextFloat(0.25f, 0.45f),
                        dmg.Damage * 0.02f);
                    _unitHealthComponent.OnDamaged += (dmg) => _cameraShakeModule.PositionShake(
                        GlobalRandom.Current.NextFloat(0.25f, 0.45f),
                        dmg.Damage * 0.02f);
                    _unitHealthComponent.OnDamaged += (dmg) => _cameraShakeModule.FovShake(
                        -2,
                        GlobalRandom.Current.NextFloat(0.25f, 0.45f),
                        dmg.Damage * 0.1f);
                }
            }
            Controller.PlayerUnit.Event.Subscribe("OnDied", () =>
            {
                Object.Destroy(Unit.gameObject);
            });
        }
        
        private Quaternion GetRotation()
        {
            return _cameraRotation;
        }
        private Vector3 GetPosition()
        {
            return _cameraPosition;
        }
        
        private float yRotation;
        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation;
        public void OnUpdate(float deltaTime)
        {
            float mouseX = Input.GetAxis("Mouse X") * _template.mouseSensitivity * 150f * deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * _template.mouseSensitivity * 150f * deltaTime;

            yRotation -= mouseY;
            yRotation = Mathf.Clamp(yRotation, -_template.maxLookAngle, _template.maxLookAngle);

            _cameraRotation = Quaternion.Euler(yRotation, Controller.PlayerUnit.transform.eulerAngles.y, 0f);
            
            Controller.PlayerUnit.transform.Rotate(Vector3.up * mouseX);
            
            _cameraPosition = Controller.PlayerUnit.transform.position + Vector3.up;
        }
        
        public Ray GetRay() => new Ray(Controller.Camera.transform.position, Controller.Camera.transform.forward);
    }
    
    [Serializable]
    public struct CameraFPS : CameraModuleTemplate
    {
        public float mouseSensitivity;
        public float maxLookAngle;
        public Type GetModuleType() => typeof(CameraFPSModule);
        public CameraModule GetModuleInstance() => new CameraFPSModule().Construct(this);
    }
}