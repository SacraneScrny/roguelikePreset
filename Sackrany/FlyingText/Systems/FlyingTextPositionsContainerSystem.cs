using Unity.Burst;
using Unity.Entities;

namespace Sackrany.FlyingText.Systems
{
    public partial struct FlyingTextPositionsContainerSystem : ISystem
    {
        private Entity _container;
        
        [BurstDiscard]
        public void OnCreate(ref SystemState state)
        {
            _container = state.EntityManager.CreateEntity(typeof(FlyingTextComponents.FlyingTextPositionBuffer));
            state.RequireForUpdate<FlyingTextComponents.EndPosition>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingletonBuffer<FlyingTextComponents.FlyingTextPositionBuffer>();
            buffer.Clear();
            foreach (var (pos, dist) in SystemAPI.Query<RefRO<FlyingTextComponents.EndPosition>, RefRO<FlyingTextComponents.DistanceToCamera>>()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>())
            {
                buffer.Add(new FlyingTextComponents.FlyingTextPositionBuffer()
                    { Value = pos.ValueRO.Value, Radius = dist.ValueRO.Value * 0.05f });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}