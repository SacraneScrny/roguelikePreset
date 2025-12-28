using System;
using System.Collections;

using Sackrany.Unit.Abstracts;
using Sackrany.Unit.ModuleSystem.Main;
using UnityEngine;

namespace Sackrany.Unit.Features.AIFeature
{
    [Serializable]
    public sealed class AIModuleController : UpdatableModuleController<AIModule, AIModuleTemplate>
    {
        private bool _playerFound = false;
        [HideInInspector] public UnitBase PlayerUnit;
        [HideInInspector] public UnitBase PlayerCameraUnit;
        
        private float _distanceToPlayer;
        private float _cameraToUnitDirectionAngle;
        public float CameraToUnitDirectionAngle => _cameraToUnitDirectionAngle;
        public float DistanceToPlayer => _distanceToPlayer;
        
        private protected override void OnInitialized(UnitBase unit)
        {
            unit.StartCoroutine(FindPlayerUnit());
        }
        
        private protected override void OnControllerUpdate(float deltaTime)
        {
            _distanceToPlayer = Vector3.Distance(CurrentUnit.transform.position, PlayerUnit.transform.position);
            _cameraToUnitDirectionAngle = Vector3.Angle(-CurrentUnit.transform.forward, PlayerCameraUnit.transform.forward);
        }
        
        private IEnumerator FindPlayerUnit()
        {
            while (!UnitManager.HasUnits(x => x.Tag.HasTag("Player"))
                   || !UnitManager.HasUnits(x => x.Tag.HasTag("PlayerCamera")))
                yield return null;
            PlayerUnit = UnitManager.GetUnit((x) => x.Tag.HasTag("Player"));
            PlayerCameraUnit = UnitManager.GetUnit((x) => x.Tag.HasTag("PlayerCamera"));
            _playerFound = true;
        }
        private protected override bool UpdateCondition() => _playerFound;
    }

    public class AIModule : LinkedModule<AIModule, AIModuleController>
    {
        private protected UnitBase PlayerUnit => Controller.PlayerUnit;
        private protected UnitBase PlayerCameraUnit => Controller.PlayerCameraUnit;
        public float DistanceToPlayer => Controller.DistanceToPlayer;
        public float CameraToUnitDirectionAngle => Controller.CameraToUnitDirectionAngle;
    }

    public interface AIModuleTemplate : ITemplate<AIModule>
    {
        
    }
}