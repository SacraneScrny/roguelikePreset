using Sackrany.CameraEntity;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Sackrany.FlyingText.Systems
{
    [UpdateAfter(typeof(FlyingTextPositionsContainerSystem))]
    public partial struct FlyingTextResetSystem : ISystem
    {
        private ComponentLookup<FlyingTextComponents.Properties> _propertiesLookup;
        private ComponentLookup<FlyingTextComponents.DistanceToCamera> _distanceToCameraLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _propertiesLookup = SystemAPI.GetComponentLookup<FlyingTextComponents.Properties>();
            _distanceToCameraLookup = SystemAPI.GetComponentLookup<FlyingTextComponents.DistanceToCamera>();
            state.RequireForUpdate<FlyingTextComponents.FlyingTextResetEvent>();
            state.RequireForUpdate<FlyingTextComponents.FlyingTextPositionBuffer>();
            state.RequireForUpdate<CameraRight>();
            state.RequireForUpdate<CameraUp>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _propertiesLookup.Update(ref state);
            _distanceToCameraLookup.Update(ref state);
            var ecbES = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var rnd = new Random((uint)(SystemAPI.Time.ElapsedTime * 1000) + 13531561);
            var posBuffer = SystemAPI.GetSingletonBuffer<FlyingTextComponents.FlyingTextPositionBuffer>();
            
            var cameraRight = SystemAPI.GetSingleton<CameraRight>();
            var cameraUp = SystemAPI.GetSingleton<CameraUp>();
            
            foreach (var e in SystemAPI.Query<RefRO<FlyingTextComponents.FlyingTextResetEvent>>())
            {
                if (!_propertiesLookup.TryGetComponent(e.ValueRO.Entity, out var properties)) continue;
                if (!_distanceToCameraLookup.TryGetComponent(e.ValueRO.Entity, out var distanceToCamera)) continue;
                
                ecbES.SetComponentEnabled<FlyingTextComponents.IsFlyingTextEnabled>(e.ValueRO.Entity, true);
                ecbES.SetComponentEnabled<FlyingTextComponents.IsImpacting>(e.ValueRO.Entity, true);
                ecbES.SetComponentEnabled<FlyingTextComponents.IsDying>(e.ValueRO.Entity, false);
                ecbES.SetComponentEnabled<FlyingTextComponents.IsDead>(e.ValueRO.Entity, false);
                
                ecbES.SetComponent(e.ValueRO.Entity, new FlyingTextComponents.TextEffect()
                {
                    Value = e.ValueRO.Effect,
                    AdditionalPosition = float3.zero,
                    AdditionalRotation = quaternion.identity,
                    AdditionalScale = float3.zero,
                });
                
                ecbES.SetComponent(e.ValueRO.Entity, new FlyingTextComponents.StartPosition()
                {
                    Value = e.ValueRO.Position
                });

                float3 futurePos =
                    e.ValueRO.Position + e.ValueRO.Direction *
                    rnd.NextFloat(properties.PushDistance.x, properties.PushDistance.y)
                    * distanceToCamera.Value * 0.1f;
                futurePos = FindFreeRelativeToView(
                    futurePos, 
                    cameraRight.Value,
                    cameraUp.Value,
                    posBuffer.AsNativeArray(), 
                    0.05f, 1000);
                
                ecbES.SetComponent(e.ValueRO.Entity, new FlyingTextComponents.EndPosition()
                {
                    Value = futurePos
                });
                
                ecbES.SetComponent(e.ValueRO.Entity, new FlyingTextComponents.TextOpacity()
                {
                    Value = 0f
                });
                ecbES.SetComponent(e.ValueRO.Entity, new FlyingTextComponents.LifeTime()
                {
                    Value = 0f,
                    Max = rnd.NextFloat(properties.LifeTime.x, properties.LifeTime.y),
                });
                ecbES.SetComponent(e.ValueRO.Entity, new FlyingTextComponents.Impact()
                {
                    Value = 0f,
                });
                ecbES.SetComponent(e.ValueRO.Entity, new FlyingTextComponents.Fade()
                {
                    Value = properties.FadeOutDuration,
                });
            }
        }
        
        bool IsFreeOnPlane(
            float3 pos,
            float3 planeNormal,
            float3 planePoint,
            NativeArray<FlyingTextComponents.FlyingTextPositionBuffer> buffer)
        {
            float distToPlane = math.dot(pos - planePoint, planeNormal);
            pos -= planeNormal * distToPlane;

            for (int i = 0; i < buffer.Length; i++)
            {
                float minDist = buffer[i].Radius;
                if (math.distancesq(pos, buffer[i].Value) < minDist * minDist)
                    return false;
            }

            return true;
        }
        float3 FindFreeRelativeToView(
            float3 origin,
            float3 camRight,
            float3 camUp,
            NativeArray<FlyingTextComponents.FlyingTextPositionBuffer> buffer,
            float step,
            int maxSteps)
        {
            var rnd = new Random((uint)math.round(math.abs(origin.x * 1000 + origin.y * 1000 + origin.z * 1000)));
            camRight = math.normalize(camRight);
            camUp    = math.normalize(camUp);
            float3 camForward = math.normalize(math.cross(camRight, camUp));

            float angle = 0f;
            float radius = 0f;

            for (int i = 0; i < maxSteps; i++)
            {
                float x = math.cos(angle) * radius;
                float y = math.sin(angle) * radius;

                float3 pos =
                    origin +
                    camRight * x +
                    camUp    * y;

                float d = math.dot(pos - origin, camForward);
                pos -= camForward * d;

                if (IsFreeOnPlane(pos, camForward, origin, buffer))
                    return pos;

                angle  += 0.5f * rnd.NextFloat(0.75f, 1.5f);
                radius += step * rnd.NextFloat(0.2f, 0.8f);
            }

            return origin;
        }



        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}