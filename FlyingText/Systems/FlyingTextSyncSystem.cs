using Sackrany.Extensions;
using Sackrany.Pool.Extensions;

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace Sackrany.FlyingText.Systems
{
    public partial struct FlyingTextSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyingTextComponents.Sync>();
        }

        [BurstDiscard]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (pos, rot, scale, rotOffset, opacity, refer, distanceToCamera) in SystemAPI.Query<
                         RefRO<FlyingTextComponents.CurrentPosition>,
                         RefRO<FlyingTextComponents.CurrentRotation>,
                         RefRO<FlyingTextComponents.CurrenScale>,
                         RefRO<FlyingTextComponents.RotationOffset>,
                         RefRO<FlyingTextComponents.TextOpacity>,
                         RefRO<FlyingTextComponents.Sync>,
                         RefRO<FlyingTextComponents.DistanceToCamera>
                     >()
                     .WithAll<FlyingTextComponents.IsFlyingTextEnabled>()
                     .WithDisabled<FlyingTextComponents.IsDead>())
            {
                refer.ValueRO.Value.Value.transform.position = pos.ValueRO.Value;
                refer.ValueRO.Value.Value.transform.rotation = math.mul(rotOffset.ValueRO.Value, rot.ValueRO.Value);
                refer.ValueRO.Value.Value.transform.localScale = scale.ValueRO.Value * distanceToCamera.ValueRO.Value * 0.06f;
                refer.ValueRO.Value.Value.Text.color = refer.ValueRO.Value.Value.Text.color.SetAlpha(opacity.ValueRO.Value);
            }

            foreach (var (effect, refer) in SystemAPI
                         .Query<RefRO<FlyingTextComponents.TextEffect>, RefRO<FlyingTextComponents.Sync>>()
                         .WithAll<FlyingTextComponents.IsFlyingTextEnabled>()
                         .WithDisabled<FlyingTextComponents.IsDead>())
            {
                refer.ValueRO.Value.Value.transform.position += (Vector3)effect.ValueRO.AdditionalPosition;
                refer.ValueRO.Value.Value.transform.rotation = math.mul(refer.ValueRO.Value.Value.transform.rotation, effect.ValueRO.AdditionalRotation);
                refer.ValueRO.Value.Value.transform.localScale += (Vector3)effect.ValueRO.AdditionalScale;
            }

            foreach (var (refer, entity) in SystemAPI.Query<RefRO<FlyingTextComponents.Sync>>()
                         .WithAll<FlyingTextComponents.IsDead>()
                         .WithEntityAccess())
            {
                SystemAPI.SetComponentEnabled<FlyingTextComponents.IsFlyingTextEnabled>(entity, false);
                SystemAPI.SetComponentEnabled<FlyingTextComponents.IsImpacting>(entity, false);
                SystemAPI.SetComponentEnabled<FlyingTextComponents.IsDying>(entity, false);
                SystemAPI.SetComponentEnabled<FlyingTextComponents.IsDead>(entity, false);
                
                refer.ValueRO.Value.Value.gameObject.RELEASE();
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}