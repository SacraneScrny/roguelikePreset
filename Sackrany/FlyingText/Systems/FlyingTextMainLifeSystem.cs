using Unity.Burst;
using Unity.Entities;

namespace Sackrany.FlyingText.Systems
{
    public partial struct FlyingTextMainLifeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyingTextComponents.IsFlyingTextEnabled>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (lifeTime, opacity, entity) in SystemAPI.Query<
                             RefRW<FlyingTextComponents.LifeTime>,
                             RefRW<FlyingTextComponents.TextOpacity>
                         >()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>()
                         .WithDisabled<FlyingTextComponents.IsImpacting>()
                         .WithDisabled<FlyingTextComponents.IsDying>()
                         .WithEntityAccess())
            {
                opacity.ValueRW.Value = 1;
                lifeTime.ValueRW.Value += SystemAPI.Time.DeltaTime;
                if (lifeTime.ValueRO.Value > lifeTime.ValueRO.Max)
                {    
                    SystemAPI.SetComponentEnabled<FlyingTextComponents.IsDying>(entity, true);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}