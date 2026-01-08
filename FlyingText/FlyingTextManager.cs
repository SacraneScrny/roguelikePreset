using System.Collections.Generic;

using Sackrany.Numerics;
using Sackrany.Utils;

using Unity.Entities;

using UnityEngine;

namespace Sackrany.FlyingText
{
    public class FlyingTextManager : AManager<FlyingTextManager>
    {
        public float TextRadius = 0.04f;
        public Vector2Int ExponentToColorGradient;
        public Gradient BigNumberToColorGradient;
        private protected override void OnManagerAwake()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = em.CreateEntity(typeof(FlyingTextProperties));
            em.SetComponentData(entity, new FlyingTextProperties()
            {
                TextRadius = TextRadius,
            });
        }
        public static Color GetBigNumberColor(BigNumber number)
        {
            if (number.Exponent < Instance.ExponentToColorGradient.x) return Instance.BigNumberToColorGradient.Evaluate(0);
            if (number.Exponent >= Instance.ExponentToColorGradient.y) return Instance.BigNumberToColorGradient.Evaluate(1f);
            return Instance.BigNumberToColorGradient.Evaluate((number.Exponent + Instance.ExponentToColorGradient.x + 1) / (float)Instance.ExponentToColorGradient.y);
        }
    }

    public struct FlyingTextProperties : IComponentData
    {
        public float TextRadius;
    }
}