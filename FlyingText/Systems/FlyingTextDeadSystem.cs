using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Sackrany.FlyingText.Systems
{
    public partial struct FlyingTextDeadSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyingTextComponents.IsDying>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (opacity, scale, fade, properties, entity) in SystemAPI.Query<
                             RefRW<FlyingTextComponents.TextOpacity>,
                             RefRW<FlyingTextComponents.CurrenScale>,
                             RefRW<FlyingTextComponents.Fade>,
                             RefRO<FlyingTextComponents.Properties>
                         >()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>()
                         .WithAll<FlyingTextComponents.IsDying>()
                         .WithDisabled<FlyingTextComponents.IsDead>()
                         .WithEntityAccess())
            {
                float t = math.saturate(fade.ValueRO.Value / properties.ValueRO.FadeOutDuration);
                opacity.ValueRW.Value = t - 0.5f;
                scale.ValueRW.Value = new float3(1, 1, 1) * t;
                fade.ValueRW.Value -= SystemAPI.Time.DeltaTime;
                if (fade.ValueRO.Value <= 0)
                {
                    SystemAPI.SetComponentEnabled<FlyingTextComponents.IsDead>(entity, true);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}