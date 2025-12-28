using System.Collections.Generic;

using Sackrany.Numerics;
using Sackrany.Utils;

using UnityEngine;

namespace Sackrany.FlyingText
{
    public class FlyingTextManager : AManager<FlyingTextManager>
    {
        public int ScatterSamples = 10;
        public float ScatterRadius = 0.4f;
        public float ScatterStrength = 1.2f;

        public Gradient BigNumberToColorGradient;
        private readonly List<FlyingText> _currentFlyingTextComponents = new List<FlyingText>();

        public static Color GetBigNumberColor(BigNumber number)
        {
            if (number.Exponent < 0) return Instance.BigNumberToColorGradient.Evaluate(0);
            if (number.Exponent > 19) return Instance.BigNumberToColorGradient.Evaluate(1f);
            return Instance.BigNumberToColorGradient.Evaluate((number.Exponent + 1) / 20f);
        }
        public Vector3 GetCameraScatterOffset(
            Vector3 worldPosition,
            Transform camera)
        {
            Vector3 offset = Vector3.zero;
            int count = 0;

            Vector3 camRight = camera.right;
            Vector3 camUp = camera.up;

            for (int i = _currentFlyingTextComponents.Count - 1;
                 i >= 0 && count < ScatterSamples;
                 i--)
            {
                var other = _currentFlyingTextComponents[i];
                if (other == null) continue;

                Vector3 delta = worldPosition - other.transform.position;

                float x = Vector3.Dot(delta, camRight);
                float y = Vector3.Dot(delta, camUp);

                Vector2 screenDelta = new Vector2(x, y);
                float dist = screenDelta.magnitude;

                if (dist > ScatterRadius)
                    continue;

                if (dist < 0.001f)
                    screenDelta = Random.insideUnitCircle * 0.001f;

                float force = 1f - (dist / ScatterRadius);
                offset +=
                    (camRight * screenDelta.x + camUp * screenDelta.y).normalized
                    * force;

                count++;
            }

            return offset * ScatterStrength;
        }
        
        public static void Register(FlyingText component) => Instance._currentFlyingTextComponents.Add(component);
        public static void Unregister(FlyingText component) => Instance._currentFlyingTextComponents.Remove(component);
    }
}