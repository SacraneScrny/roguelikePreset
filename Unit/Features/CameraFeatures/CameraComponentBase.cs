using System;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Unit.Abstracts;
using Sackrany.Unit.ModuleSystem.Main;

using UnityEngine;

namespace Sackrany.Unit.Features.CameraFeatures
{
    [Serializable]
    public sealed class CameraComponentsController : UpdatableModuleController<CameraModule, CameraModuleTemplate>
    {
        public Camera Camera;
        public UnitBase PlayerUnit;
        
        public ExpandedFloat CameraFov = new ExpandedFloat(80);
        public ExpandedVector3 CameraPosition = new ExpandedVector3(Vector3.zero);
        public ExpandedQuaternion CameraRotation = new ExpandedQuaternion(Quaternion.identity);
    }

    [Serializable]
    public class CameraModule : LinkedModule<CameraModule, CameraComponentsController>
    {
        
    }

    public interface CameraModuleTemplate : ITemplate<CameraModule>
    {
        
    }
}