using System;

using Sackrany.CustomRandom.Global;
using Sackrany.ExpandedVariable.Abstracts;
using Sackrany.Unit.Features.ComponentsFeature.Modules;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

namespace Sackrany.Unit.Features.CameraFeatures.Modules
{
    public class CameraFPSModule : CameraModule, IUpdateModule
    {
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

            if (TryGet(out CameraShakeModule shake))
            {
                if (Controller.PlayerUnit.TryGetModule(out UnitMovementComponent movement))
                {
                    movement.OnGrounded += (vel) => shake.RotationShake(
                        new Vector3(9, 0, 0),
                        GlobalRandom.Current.NextFloat(0.25f, 0.45f),
                        vel * 0.1f);
                    movement.OnGrounded += (vel) => shake.PositionShake(
                        Vector3.down * 1.5f,
                        GlobalRandom.Current.NextFloat(0.1f, 0.15f),
                        vel * 0.6f);
                    movement.OnGrounded += (vel) => shake.FovShake(
                        -2,
                        GlobalRandom.Current.NextFloat(0.1f, 0.15f),
                        vel * 5f);
                }

                if (Controller.PlayerUnit.TryGetModule(out UnitHealthComponent health))
                {
                    health.OnDamaged += (dmg) => shake.RotationShake(
                        GlobalRandom.Current.NextFloat(0.25f, 0.45f),
                        dmg.Damage * 0.02f);
                    health.OnDamaged += (dmg) => shake.PositionShake(
                        GlobalRandom.Current.NextFloat(0.25f, 0.45f),
                        dmg.Damage * 0.02f);
                    health.OnDamaged += (dmg) => shake.FovShake(
                        -2,
                        GlobalRandom.Current.NextFloat(0.25f, 0.45f),
                        dmg.Damage * 0.1f);
                }
            }
            Controller.PlayerUnit.Event.Subscribe("OnDied", () =>
            {
                GameObject.Destroy(Unit.gameObject);
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