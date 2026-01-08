using System;

using Sackrany.CMS;
using Sackrany.CustomRandom.Global;
using Sackrany.Pool.Extensions;
using Sackrany.Unit.ModuleSystem;
using Sackrany.Unit.ModuleSystem.Updates;

using UnityEngine;

namespace Sackrany.Unit.Features.ComponentsFeature.Modules
{
    public class UnitHitReactionComponent : UnitComponent, IUpdateModule
    {
        [Dependency] private UnitHealthComponent healthComponent;
        private UnitHitReaction _template;
        public UnitHitReactionComponent Construct(UnitHitReaction template)
        {
            _template = template;
            return this;
        }
        protected override void OnStart()
        {
            if (TryGet(out UnitMovementComponent movement))
            {
                healthComponent.OnDamaged += (x)
                    => movement.AdditionalVelocity += x.Direction.normalized * x.Damage * _template.reactionPower;

                healthComponent.OnDamaged += (x) =>
                {
                    var sound = CMS_Manager.CMS
                        .Get(_template.soundEffects[GlobalRandom.Current.Next(_template.soundEffects.Length)]).POOL();
                    sound.transform.position = x.Position;
                };
                healthComponent.OnDamaged += (x) =>
                {
                    var vfx = CMS_Manager.CMS
                        .Get(_template.particleEffects[GlobalRandom.Current.Next(_template.particleEffects.Length)]).POOL();
                    vfx.transform.position = Unit.transform.position;
                };
                
                healthComponent.OnDamaged += (x) => hitScale += new Vector3(
                    GlobalRandom.Current.NextFloat(),
                    GlobalRandom.Current.NextFloat(),
                    GlobalRandom.Current.NextFloat()
                    ).normalized * x.Damage * _template.reactionPower;
            }

            Controller.UnitScale.Add_PostAdditional(() => hitScale);
        }

        private Vector3 hitScale;
        public void OnUpdate(float deltaTime)
        {
            hitScale = Vector3.Lerp(hitScale, Vector3.zero, deltaTime * 15f);
        }
    }
    
    [Serializable]
    public struct UnitHitReaction : IUnitComponentTemplate
    {
        public float reactionPower;
        public HashCMS[] soundEffects;
        public HashCMS[] particleEffects;
        public Type GetModuleType() => typeof(UnitHitReactionComponent);
        public UnitComponent GetModuleInstance() => new UnitHitReactionComponent().Construct(this);
    }
}