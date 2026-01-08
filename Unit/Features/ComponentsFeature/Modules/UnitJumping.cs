using System;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Unit.ModuleSystem;
using Sackrany.Unit.ModuleSystem.Updates;

using Unity.Mathematics;

using UnityEngine;

namespace Sackrany.Unit.Features.ComponentsFeature.Modules
{
    public class UnitJumpingComponent : UnitComponent, IUpdateModule
    {
        [Dependency] private CharacterController _characterController;
        [Dependency] private UnitGravityComponent _gravity;
        private float _currentJumpVelocity;
        public float CurrentJumpVelocity => _currentJumpVelocity;
        public readonly ExpandedFloat JumpForce;
        
        public UnitJumpingComponent(float jumpForce)
        {
            JumpForce = jumpForce;
        }
        protected override void OnStart()
        {
            JumpForce.Add_Multiply(() => _gravity.Gravity);
        }
        protected override void OnReset()
        {
            OnJump = null;
            JumpForce.Clear();
        }

        private float jumpDeadTime;
        public void Jump()
        {
            if (jumpDeadTime > 0) return;
            if (!_characterController.isGrounded) return;
            jumpDeadTime = 0.2f;
            _currentJumpVelocity = Mathf.Sqrt(-2f * JumpForce);
            OnJump?.Invoke();
        }
        public void OnUpdate(float deltaTime)
        {
            jumpDeadTime -= deltaTime;
            if (_currentJumpVelocity <= 0)
            {
                _currentJumpVelocity = 0;
                return;
            }
            _characterController.Move(Vector3.up * _currentJumpVelocity * deltaTime);
            _currentJumpVelocity += _gravity.Gravity * deltaTime;
        }
        
        public event System.Action OnJump;
    }
    
    [Serializable]
    public struct UnitJumping : IUnitComponentTemplate
    {
        public float jumpForce;
        public Type GetModuleType() => typeof(UnitJumpingComponent);
        public UnitComponent GetModuleInstance() => new UnitJumpingComponent(jumpForce);
    }
}