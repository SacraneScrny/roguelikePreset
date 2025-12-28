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
            var cameraPos = SystemAPI.GetSingleton<CameraPosition>();
            foreach (var (rotationOffset, position) in SystemAPI
                         .Query<RefRW<FlyingTextComponents.RotationOffset>,
                         RefRO<FlyingTextComponents.CurrentPosition>>()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>())
            {
                float3 forward = math.normalize(cameraPos.Value - position.ValueRO.Value);
                float3 up = math.up();
                quaternion rot = quaternion.LookRotationSafe(forward, up);

                float3 right = math.cross(up, forward);
                forward = math.cross(right, up);

                rotationOffset.ValueRW.Value = quaternion.LookRotationSafe(forward, up);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}