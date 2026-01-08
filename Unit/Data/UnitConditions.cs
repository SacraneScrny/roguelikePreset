
using System;

using UnityEngine;

namespace Sackrany.Unit.Data
{
    [Serializable]
    public class UnitConditions : AUnitData
    {
        private CharacterController _characterController;
        private Action _deltaUpdateAction;
        
        private Vector3 _lastPosition;
        private Vector3 _deltaPosition;
        private float _deltaPositionMagnitude;
        public Vector3 GetDeltaPosition => _deltaPosition;
        public float GetDeltaPositionMagnitude => _deltaPositionMagnitude;
        
        private protected override void OnInitialize()
        {
            if (_unit.TryGetComponent(out _characterController))
                _deltaUpdateAction = () =>
                {
                    _deltaPosition = _characterController.velocity * Time.fixedDeltaTime;
                    _deltaPositionMagnitude = _deltaPosition.magnitude;
                };
            else
                _deltaUpdateAction = () =>
                {
                    _deltaPosition = (_unit.transform.position - _lastPosition) * Time.fixedDeltaTime;
                    _lastPosition = _unit.transform.position;
                    _deltaPositionMagnitude = _deltaPosition.magnitude;
                };
        }
        
        public void Update()
        {
        }        
        public void FixedUpdate()
        {
            _deltaUpdateAction();
        }
        
        public override void Reset()
        {
            
        }
    }
}