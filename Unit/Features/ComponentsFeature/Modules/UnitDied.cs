using System;
using System.Collections;

using Sackrany.CMS;
using Sackrany.CustomRandom.Global;
using Sackrany.Pool.Extensions;

using Object = UnityEngine.Object;

namespace Sackrany.Unit.Features.ComponentsFeature.Modules
{
    public class UnitDiedComponent : UnitComponent
    {
        private UnitDied _template;
        public UnitDiedComponent Construct(UnitDied template)
        {
            _template = template;
            return this;
        }
        protected override void OnStart()
        {
            Unit.Event.Subscribe("OnDied", () =>
            {
                var effect = CMS_Manager.CMS.Get(_template.particles[GlobalRandom.Current.Next(_template.particles.Length)]).POOL();
                effect.transform.position = Unit.transform.position;
                effect.transform.rotation = Unit.transform.rotation;
                Unit.StartCoroutine(destroyEnum());
            });
        }

        IEnumerator destroyEnum()
        {
            yield return null;
            Object.Destroy(Unit.gameObject);
        }
    }

    [Serializable]
    public struct UnitDied : IUnitComponentTemplate
    {
        public HashCMS[] particles;
        public Type GetModuleType() => typeof(UnitDiedComponent);
        public UnitComponent GetModuleInstance() => new UnitDiedComponent().Construct(this);
    }
}