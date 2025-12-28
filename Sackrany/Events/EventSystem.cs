using Unity.Burst;
using Unity.Entities;

using UnityEngine;

namespace Logic.Events
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct EventSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EventComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb =
                SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (eventComponent, entity) in SystemAPI.Query<RefRO<EventComponent>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
        }
        
        public void OnDestroy(ref SystemState state)
        {
            
        }
    }

    public struct EventComponent : IComponentData
    {
        public Entity CalledFrom;
    }

    public static class EventHelper
    {
        /// <summary>
        /// CREATE ON END SIMULATION
        /// </summary>
        public static Entity CreateEvent<T>(EntityCommandBuffer ecb, Entity calledFrom, T component) where T : unmanaged, IComponentData
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new EventComponent { CalledFrom = calledFrom });
            ecb.AddComponent(entity, component);
            return entity;
        }
    }
}