using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

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
            state.RequireForUpdate<FlyingTextProperties>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var prop = SystemAPI.GetSingleton<FlyingTextProperties>();
            var buffer = SystemAPI.GetSingletonBuffer<FlyingTextComponents.FlyingTextPositionBuffer>();
            buffer.Clear();
            foreach (var (pos, dist, effect) in SystemAPI.Query<
                             RefRO<FlyingTextComponents.EndPosition>, 
                             RefRO<FlyingTextComponents.DistanceToCamera>,
                         RefRO<FlyingTextComponents.TextEffect>>()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>())
            {
                buffer.Add(new FlyingTextComponents.FlyingTextPositionBuffer()
                {
                    Value = pos.ValueRO.Value, 
                    Radius = dist.ValueRO.Value * prop.TextRadius * math.select(1, 2.2f, effect.ValueRO.Value == Effect.Crit)
                });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}