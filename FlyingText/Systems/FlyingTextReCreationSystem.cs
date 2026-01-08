using Sackrany.CameraEntity;
using Sackrany.Events;

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Sackrany.FlyingText.Systems
{
    public partial struct FlyingTextReCreationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyingTextComponents.Sync>();
            state.RequireForUpdate<CameraPosition>();
        } 

        [BurstDiscard]
        public void OnUpdate(ref SystemState state)
        {
            var ecbES = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var cameraPos = SystemAPI.GetSingleton<CameraPosition>();
            foreach (var (sync, distanceToCamera, entity) in SystemAPI.Query<RefRO<FlyingTextComponents.Sync>, RefRW<FlyingTextComponents.DistanceToCamera>>()
                         .WithDisabled<FlyingTextComponents.IsFlyingTextEnabled>()
                         .WithEntityAccess()) 
            {  
                if (!sync.ValueRO.Value.Value.IsRecreated) continue;  
                distanceToCamera.ValueRW.Value = math.distance(sync.ValueRO.Value.Value.Pos, cameraPos.Value);
                EventHelper.CreateEvent(ecbES, entity, new FlyingTextComponents.FlyingTextResetEvent()
                {
                    Position = sync.ValueRO.Value.Value.Pos,
                    Direction = sync.ValueRO.Value.Value.Dir,
                    Entity = entity,
                    Effect = sync.ValueRO.Value.Value.CurrentEffect,
                });
                sync.ValueRO.Value.Value.IsRecreated = false;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}