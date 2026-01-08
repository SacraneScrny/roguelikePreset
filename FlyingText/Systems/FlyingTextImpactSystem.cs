using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Sackrany.FlyingText.Systems
{
    public partial struct FlyingTextImpactSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyingTextComponents.IsImpacting>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (impact, opacity, properties) in SystemAPI.Query<
                         RefRO<FlyingTextComponents.Impact>,
                         RefRW<FlyingTextComponents.TextOpacity>,
                         RefRO<FlyingTextComponents.Properties>
                     >()
                     .WithAll<FlyingTextComponents.IsImpacting>()
                     .WithAll<FlyingTextComponents.IsFlyingTextEnabled>())
            {
                float t = impact.ValueRO.Value / properties.ValueRO.ImpactDuration;
                opacity.ValueRW.Value = math.saturate(t);
            }
            
            foreach (var (impact, position, start, end, properties) in SystemAPI.Query<
                             RefRO<FlyingTextComponents.Impact>,
                             RefRW<FlyingTextComponents.CurrentPosition>,
                             RefRO<FlyingTextComponents.StartPosition>,
                             RefRO<FlyingTextComponents.EndPosition>,
                             RefRO<FlyingTextComponents.Properties> 
                         >()
                         .WithAll<FlyingTextComponents.IsImpacting>()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>())
            {
                float t = impact.ValueRO.Value / properties.ValueRO.ImpactDuration;
                float noiseFade = math.pow(1f - t, 2f);
                float time = (float)SystemAPI.Time.ElapsedTime * properties.ValueRO.NoiseFrequency;
                
                float nx = noise.snoise(new float2(2362223, time));
                float ny = noise.snoise(new float2(64463898, time));
                var offset = 
                    new float3(nx, ny, 0f) * properties.ValueRO.NoiseStrength * noiseFade;
                position.ValueRW.Value = math.lerp(
                    start.ValueRO.Value, 
                    end.ValueRO.Value, 
                    t * math.saturate(properties.ValueRO.ImpactSpeed)) + offset;
            }
            
            foreach (var (impact, scale, properties) in SystemAPI.Query<
                             RefRO<FlyingTextComponents.Impact>,
                             RefRW<FlyingTextComponents.CurrenScale>,
                             RefRO<FlyingTextComponents.Properties>
                         >()
                         .WithAll<FlyingTextComponents.IsImpacting>()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>())
            {
                float t = impact.ValueRO.Value / properties.ValueRO.ImpactDuration;
                float scalePunch =
                    math.lerp(properties.ValueRO.ImpactScale, 1f, t);
                scale.ValueRW.Value = new float3(1, 1, 1) * scalePunch * 1.4f;
            }
            
            foreach (var (impact, rotation, properties) in SystemAPI.Query<
                             RefRO<FlyingTextComponents.Impact>, 
                             RefRW<FlyingTextComponents.CurrentRotation>,
                             RefRO<FlyingTextComponents.Properties>
                         >()
                         .WithAll<FlyingTextComponents.IsImpacting>()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>())
            {
                float t = impact.ValueRO.Value / properties.ValueRO.ImpactDuration;
                rotation.ValueRW.Value = quaternion.Euler(0, 0, 
                    (1f - t) * properties.ValueRO.ImpactRotation);
            }

            foreach (var (impact, position, rotation, scale, endPosition, properties, entity) in SystemAPI.Query<
                             RefRW<FlyingTextComponents.Impact>,
                             RefRW<FlyingTextComponents.CurrentPosition>,
                             RefRW<FlyingTextComponents.CurrentRotation>,
                             RefRW<FlyingTextComponents.CurrenScale>,
                             RefRO<FlyingTextComponents.EndPosition>,
                             RefRO<FlyingTextComponents.Properties>
                         >()
                         .WithAll<FlyingTextComponents.IsImpacting>()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>()
                         .WithEntityAccess())
            {
                impact.ValueRW.Value += SystemAPI.Time.DeltaTime;
                if (impact.ValueRO.Value > properties.ValueRO.ImpactDuration)
                {
                    position.ValueRW.Value = endPosition.ValueRO.Value;
                    rotation.ValueRW.Value = quaternion.identity;
                    scale.ValueRW.Value = new float3(1, 1, 1) * 1.4f;
                    SystemAPI.SetComponentEnabled<FlyingTextComponents.IsImpacting>(entity, false);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}