using System;

using Sackrany.CMS;
using Sackrany.ExpandedVariable.Entities;
using Sackrany.Unit.Abstracts;
using Sackrany.Unit.ModuleSystem;
using Sackrany.Unit.ModuleSystem.Main;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

namespace Sackrany.Unit.Features.ComponentsFeature.Modules
{
    public class UnitMovementComponent : UnitComponent, IUpdateModule, ISerializableModule
    {
        private bool _isGrounded;
        [Template] private UnitMovement _template;
        [Dependency] private CharacterController _characterController;
        private Vector3 _currentMove;
        private bool _wasMoved;
        public bool WasMoved => _wasMoved;
        
        public Vector3 AdditionalVelocity;
        public ExpandedBool IsSprinting;
        public ExpandedFloat SprintMultiplier;
        public ExpandedFloat MoveSpeed;
        public bool IsGrounded => _isGrounded;
        public float LastMoveSpeed => _lastMoveSpeed;
        private bool _isFlying;
        
        protected override void OnAwake()
        {
            _isFlying = _template.isFlying;
            IsSprinting = false;
            AdditionalVelocity = Vector3.zero;
            SprintMultiplier = _template.sprintMultiplier;
            MoveSpeed = _template.moveSpeed;
        }
        protected override void OnReset()
        {
            OnGrounded = null;
            SprintMultiplier.Clear();
            IsSprinting.Clear();
            MoveSpeed.Clear();
        }
        
        protected override void OnStart()
        {
            _characterController = Unit.GetComponent<CharacterController>();    
        }
        
        private float _lastMoveSpeed;
        public void Move(Vector3 direction)
        {
            _lastMoveSpeed = MoveSpeed * (IsSprinting ? SprintMultiplier : 1f);
            _currentMove += (direction.normalized * _lastMoveSpeed);
            _wasMoved = true;
        }
        public void MoveRelative(Vector3 direction)
        {
            _lastMoveSpeed = MoveSpeed * (IsSprinting ? SprintMultiplier : 1f);
            _currentMove += (Unit.transform.TransformDirection(direction.normalized) * _lastMoveSpeed);
            _wasMoved = true;
        }
        
        private bool _wasGrounded;
        private float _demo = 1f;
        public void OnUpdate(float deltaTime)
        {
            _demo += deltaTime;
            float addVMgn = AdditionalVelocity.sqrMagnitude;
            if (addVMgn > float.Epsilon * 2f)
            {
                _currentMove += AdditionalVelocity;
                AdditionalVelocity -= AdditionalVelocity * deltaTime * (addVMgn + 1f);
                _wasMoved = true;
            }
            
            _isGrounded = _characterController.isGrounded || _isFlying;
            if (!_wasGrounded && _isGrounded)
            {
                _wasGrounded = true;
                OnGrounded?.Invoke(Unit.Conditions.GetDeltaPositionMagnitude);
            }
            _wasGrounded = _isGrounded;
            
            if (!_wasMoved)
            {
                return;
            }
            _characterController.Move(_currentMove * deltaTime * _demo);
            _currentMove = Vector3.Lerp(_currentMove, Vector3.zero, deltaTime * 25f);
            if (_currentMove.sqrMagnitude <= float.Epsilon * 10) _wasMoved = false;
        }

        public Action<float> OnGrounded;
        public object[] Serialize() => new object[] { (float)_demo };
        public void Deserialize(object[] data)
        {
            _demo = Convert.ToSingle(data[0]);
        }
    }
    
    [Serializable]
    public struct UnitMovement : IUnitComponentTemplate
    {
        public bool isFlying;
        public float moveSpeed;
        public float sprintMultiplier;
        public Type GetModuleType() => typeof(UnitMovementComponent);
        public UnitComponent GetModuleInstance() => new UnitMovementComponent();
    }
}