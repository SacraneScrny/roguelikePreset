using System;

using Sackrany.Pool.Extensions;
using Sackrany.Unit.ModuleSystem;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

namespace Sackrany.Unit.Features.ComponentsFeature.Modules
{
    public class BulletComponent : UnitComponent, IUpdateModule
    {
        [Template] private Bullet bullet;

        private float time;
        public void OnUpdate(float deltaTime)
        {
            Unit.gameObject.name = $"Bullet {time}";
            time += deltaTime;
            Unit.transform.Translate(Unit.transform.forward * deltaTime * bullet.speed, Space.World);
            if (time >= 3f)
                Unit.RELEASE();
        }
    }
    
    [Serializable]
    public struct Bullet : IUnitComponentTemplate
    {
        [HashKey] public float speed;
        public Type GetModuleType() => typeof(BulletComponent);
        public UnitComponent GetModuleInstance() => new BulletComponent();
    }
}