using Unity.Entities;
using Unity.Mathematics;

namespace Sackrany.FlyingText
{
    public static class FlyingTextComponents
    {
        public struct IsFlyingTextEnabled : IComponentData, IEnableableComponent { }
        public struct IsImpacting : IComponentData, IEnableableComponent { }
        public struct IsDying : IComponentData, IEnableableComponent { }
        public struct IsDead : IComponentData, IEnableableComponent { }
        
        public struct Properties : IComponentData
        {
            public float2 LifeTime;
            public float ImpactDuration;
            public float FadeOutDuration;

            public float2 PushDistance;
            public float ImpactSpeed;

            public float ImpactScale;
            public float ImpactRotation;

            public float NoiseStrength;
            public float NoiseFrequency;
        }
        public struct Sync : IComponentData
        {
            public UnityObjectRef<FlyingText> Value;
        }
        
        public struct TextEffect : IComponentData
        {
            public Effect Value;
            public float3 AdditionalPosition;
            public quaternion AdditionalRotation;
            public float3 AdditionalScale;
        }
        public struct DistanceToCamera : IComponentData
        {
            public float Value;
        }
        public struct CurrenScale : IComponentData
        {
            public float3 Value;
        }
        public struct CurrentRotation : IComponentData
        {
            public quaternion Value;
        }
        public struct RotationOffset : IComponentData
        {
            public quaternion Value;
        }
        public struct StartPosition : IComponentData
        {
            public float3 Value;
        }
        public struct EndPosition : IComponentData
        {
            public float3 Value;
        }
        public struct CurrentPosition : IComponentData
        {
            public float3 Value;
        }
        
        public struct Fade : IComponentData
        {
            public float Value;
        }
        public struct TextOpacity : IComponentData
        {
            public float Value;
        }
        public struct LifeTime : IComponentData
        {
            public float Value;
            public float Max;
        }
        public struct Impact : IComponentData
        {
            public float Value;
        }
        
        public struct FlyingTextResetEvent : IComponentData
        {
            public float3 Position;
            public float3 Direction;
            public Entity Entity;
            public Effect Effect;
        }
        
        public struct FlyingTextPositionBuffer : IBufferElementData
        {
            public float3 Value;
            public float Radius;
        }
    }
}