using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

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
            foreach (var (lifeTime, opacity, scale, entity) in SystemAPI.Query<
                             RefRW<FlyingTextComponents.LifeTime>,
                             RefRW<FlyingTextComponents.TextOpacity>,
                             RefRW<FlyingTextComponents.CurrenScale>
                         >()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>()
                         .WithDisabled<FlyingTextComponents.IsImpacting>()
                         .WithDisabled<FlyingTextComponents.IsDying>()
                         .WithEntityAccess())
            {
                opacity.ValueRW.Value = 1f - lifeTime.ValueRO.Value / lifeTime.ValueRO.Max + 0.5f;
                lifeTime.ValueRW.Value += SystemAPI.Time.DeltaTime;
                scale.ValueRW.Value = math.lerp(scale.ValueRO.Value, new float3(1, 1, 1),
                    math.pow(lifeTime.ValueRO.Value / lifeTime.ValueRO.Max, 0.3f));
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