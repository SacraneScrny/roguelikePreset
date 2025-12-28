using System;

using Sackrany.CustomRandom.Global;
using Sackrany.Extensions;
using Sackrany.Numerics;
using Sackrany.Pool.Abstracts;
using Sackrany.Pool.Extensions;
using TMPro;

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace Sackrany.FlyingText
{
    public class FlyingText : MonoBehaviour, IPoolable
    {
        [Header("Refs")]
        public TMP_Text Text;
        public SpriteRenderer SpriteRenderer;

        [Header("Timings")]
        public Vector2 LifeTime = new(0.8f, 1.2f);
        public float ImpactDuration = 0.12f;
        public float FadeOutDuration = 0.3f;

        [Header("Motion")]
        public Vector2 PushDistance = new(0.6f, 1.2f);
        public float ImpactSpeed = 18f;

        [Header("Impact FX")]
        public float ImpactScale = 1.6f;
        public float ImpactRotation = 20f;

        [Header("Noise (spawn only)")]
        public float NoiseStrength = 0.15f;
        public float NoiseFrequency = 14f;

        private Entity _entity;

        public Vector3 Pos;
        public Vector3 Dir;
        public bool IsRecreated;
        
        private void Awake()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            _entity = em.CreateEntity(
                typeof(FlyingTextComponents.IsFlyingTextEnabled),
                typeof(FlyingTextComponents.IsImpacting),
                typeof(FlyingTextComponents.IsDying),
                typeof(FlyingTextComponents.IsDead),
                typeof(FlyingTextComponents.Properties),
                typeof(FlyingTextComponents.DistanceToCamera),
                typeof(FlyingTextComponents.Sync),
                typeof(FlyingTextComponents.CurrenScale),
                typeof(FlyingTextComponents.CurrentRotation),
                typeof(FlyingTextComponents.RotationOffset),
                typeof(FlyingTextComponents.StartPosition),
                typeof(FlyingTextComponents.EndPosition),
                typeof(FlyingTextComponents.CurrentPosition),
                typeof(FlyingTextComponents.Fade),
                typeof(FlyingTextComponents.TextOpacity),
                typeof(FlyingTextComponents.LifeTime),
                typeof(FlyingTextComponents.Impact)
                );
            
            em.SetComponentEnabled<FlyingTextComponents.IsFlyingTextEnabled>(_entity, false);
            em.SetComponentEnabled<FlyingTextComponents.IsImpacting>(_entity, false);
            em.SetComponentEnabled<FlyingTextComponents.IsDying>(_entity, false);
            em.SetComponentEnabled<FlyingTextComponents.IsDead>(_entity, false);
            
            em.SetComponentData(_entity, new FlyingTextComponents.Sync() { Value = this });
            em.SetComponentData(_entity, new FlyingTextComponents.Properties()
            {
                LifeTime = LifeTime,
                ImpactDuration = ImpactDuration,
                FadeOutDuration = FadeOutDuration,
                ImpactSpeed = ImpactSpeed,
                NoiseStrength = NoiseStrength,
                NoiseFrequency = NoiseFrequency,
                PushDistance = PushDistance,
                ImpactScale = ImpactScale,
                ImpactRotation = ImpactRotation,
            });
        }

        public FlyingText Initialize(string text, Vector3 position, Vector3 direction)
        {
            Text.text = text;
            Text.color = Text.color.SetAlpha(0f);
            SpriteRenderer.color = SpriteRenderer.color.SetAlpha(0f);
            
            Pos = position;
            Dir = direction;
            IsRecreated = true;
            
            return this;
        }

        public FlyingText Initialize(float num, Vector3 position, Vector3 direction) =>
            Initialize(num.ToString("F1"), position, direction);

        public FlyingText Initialize(BigNumber num, Vector3 position, Vector3 direction)
        {
            Text.color = FlyingTextManager.GetBigNumberColor(num);
            return Initialize(num.ToShortString(), position, direction);
        }

        public void OnPooled()
        {
            gameObject.SetActive(true);
        }

        public void OnReleased()
        {
            IsRecreated = false;
            gameObject.SetActive(false);
        }
    }
}
