using Sackrany.CameraEntity;

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Sackrany.FlyingText.Systems
{
    public partial struct FlyingTextLookAtCameraSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyingTextComponents.RotationOffset>();
            state.RequireForUpdate<CameraPosition>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cameraForward = SystemAPI.GetSingleton<CameraForward>();
            var cameraUp = SystemAPI.GetSingleton<CameraUp>();
            foreach (var (rotationOffset, position) in SystemAPI
                         .Query<RefRW<FlyingTextComponents.RotationOffset>,
                         RefRO<FlyingTextComponents.CurrentPosition>>()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>())
            {
                rotationOffset.ValueRW.Value = quaternion.LookRotation(-cameraForward.Value, cameraUp.Value);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}