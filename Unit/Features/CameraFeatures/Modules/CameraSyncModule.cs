using System;

using Sackrany.Unit.ModuleSystem.Updates;

using Unity.Mathematics;

namespace Sackrany.Unit.Features.CameraFeatures.Modules
{
    public class CameraSyncModule : CameraModule, ILateUpdateModule, IUpdateModule, IFixedUpdateModule
    {
        public void OnLateUpdate(float deltaTime)
        {
            Controller.Camera.transform.position = Controller.CameraPosition;
            Controller.Camera.transform.rotation = Controller.CameraRotation;
            Controller.Camera.fieldOfView = math.clamp(Controller.CameraFov, 40, 130);
        }
        public void OnUpdate(float deltaTime)
        {
        }
        public void OnFixedUpdate(float deltaTime)
        {
        }
    }

    [Serializable]
    public struct CameraSync : CameraModuleTemplate
    {
        public Type GetModuleType() => typeof(CameraSyncModule);
        public CameraModule GetModuleInstance() => new CameraSyncModule();
    }
}