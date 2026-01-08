using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Sackrany.FlyingText.Systems
{
    public partial struct FlyingTextEffectSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyingTextComponents.TextEffect>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (effect, lifeTime, impact, fade, properties) in SystemAPI.Query<
                         RefRW<FlyingTextComponents.TextEffect>,
                         RefRO<FlyingTextComponents.LifeTime>,
                         RefRO<FlyingTextComponents.Impact>,
                         RefRO<FlyingTextComponents.Fade>, 
                         RefRO<FlyingTextComponents.Properties>
                     >()
                     .WithAll<FlyingTextComponents.IsFlyingTextEnabled>()
                     .WithDisabled<FlyingTextComponents.IsDead>())
            {
                float impact_t = impact.ValueRO.Value / properties.ValueRO.ImpactDuration;
                float lifetime_t = lifeTime.ValueRO.Value / lifeTime.ValueRO.Max;
                float fade_t = 1f - fade.ValueRO.Value / properties.ValueRO.FadeOutDuration;
                
                switch (effect.ValueRO.Value)
                {
                    case Effect.Default: continue;
                    case Effect.Crit:
                        CritEffect(effect, impact_t, lifetime_t, fade_t, SystemAPI.Time.DeltaTime);
                        continue;
                }
            }
        }

        private void CritEffect(
            RefRW<FlyingTextComponents.TextEffect> effect,
            float lifeTime,
            float impact,
            float fade,
            float dt)
        {
            float im = math.saturate(impact);
            float lt = math.saturate(lifeTime);
            float fd = math.saturate(fade);

            float punch = math.pow(im, 0.12f);

            float scale =
                1.0f
                + punch * 3.2f
                - lt * 0.7f
                - fd * 0.9f;

            scale = math.max(scale, 0.35f);

            effect.ValueRW.AdditionalScale =
                new float3(0.25f, 0.25f, 0.25f) * scale;

            float posShake = punch * 20f * math.exp(-lt * 5f);

            float t = (im + lt) * 100f; 

            float2 posNoise = new float2(
                noise.snoise(new float2(t, 12.7f)),
                noise.snoise(new float2(t + 41.3f, 91.9f))
            );

            effect.ValueRW.AdditionalPosition =
                new float3(posNoise * posShake, 0f);

            float rotStrength =
                punch * 18f * math.exp(-lt * 5f);

            float noiseVal =
                noise.snoise(new float2(t * 1.7f, 123.456f));

            float angularVel =
                math.radians(noiseVal * rotStrength);
            float deltaAngle = angularVel * dt * 1500;

            effect.ValueRW.AdditionalRotation =
                math.mul(
                    effect.ValueRW.AdditionalRotation,
                    quaternion.AxisAngle(
                        new float3(1f, 1f, 1f),
                        deltaAngle
                    )
                );
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}